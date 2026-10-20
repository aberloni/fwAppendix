using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace fwp.scenes.ed
{
    public class EdScenesUtils
    {
        public static bool CheckAndPromptUnsavedScenes()
        {
            Scene[] openScenes = new Scene[SceneManager.sceneCount];
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                openScenes[i] = SceneManager.GetSceneAt(i);
            }

            List<Scene> dirtyScenes = new();
            foreach (Scene scene in openScenes)
            {
                if (scene.isDirty)
                {
                    dirtyScenes.Add(scene);
                }
            }

            if (dirtyScenes.Count == 0)
            {
                return true;
            }

            string sceneList = string.Join(", ", System.Array.ConvertAll(dirtyScenes.ToArray(), s => s.name));

            int option = EditorUtility.DisplayDialogComplex(
                "Unsaved Scene Changes",
                $"Scenes with unsaved changes:\n{sceneList}\n\nSave before loading new scene?",
                "Save All",
                "Don't Save",
                "Cancel"
            );

            switch (option)
            {
                case 0: // Save All
                    foreach (Scene scene in dirtyScenes)
                    {
                        EditorSceneManager.SaveScene(scene);
                    }
                    return true;
                case 1: // Don't Save
                    return true;
                case 2: // Cancel
                    return false;
            }

            return false;
        }

        /// <summary>
        /// all scenes currently opened in editor
        /// </summary>
        public static HashSet<Scene> GetOpenedScenes()
        {
            HashSet<Scene> output = new();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                output.Add(SceneManager.GetSceneAt(i));
            }
            return output;
        }

        /// <summary>
        /// fold up in hierarchy all scenes opened since given snapshot
        /// (snapshot from GetOpenedScenes)
        /// </summary>
        public static void FoldScenesOpenedSince(HashSet<Scene> scenesBefore)
        {
            List<Scene> scenes = new();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene sc = SceneManager.GetSceneAt(i);
                if (!scenesBefore.Contains(sc)) scenes.Add(sc);
            }

            if (scenes.Count <= 0) return;

            // hierarchy tree is rebuilt on next repaint
            // (and unity expands freshly opened scenes)
            EditorApplication.delayCall += () => FoldScenesInHierarchy(scenes);
        }

        /// <summary>
        /// no public api : reflection on internal SceneHierarchyWindow
        /// SetExpandedRecursive(id, bool)
        /// id is int (< unity 6) or EntityId (unity 6+)
        /// scene tree item id = scene.handle (converted)
        /// </summary>
        public static void FoldScenesInHierarchy(List<Scene> scenes)
        {
            System.Type t = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            if (t == null)
            {
                Debug.LogWarning("fold hierarchy : no SceneHierarchyWindow type ?");
                return;
            }

            MethodInfo method = null;
            foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (m.Name != "SetExpandedRecursive") continue;
                var ps = m.GetParameters();
                if (ps.Length == 2 && ps[1].ParameterType == typeof(bool))
                {
                    method = m;
                    break;
                }
            }

            if (method == null)
            {
                Debug.LogWarning("fold hierarchy : no SceneHierarchyWindow.SetExpandedRecursive(id, bool) in this unity version ?");
                return;
            }

            System.Type idType = method.GetParameters()[0].ParameterType;

            // don't use GetWindow : would open/focus a hierarchy
            Object[] windows = Resources.FindObjectsOfTypeAll(t);

            foreach (Scene sc in scenes)
            {
                if (!sc.IsValid()) continue;

                object id = sceneToTreeId(sc, idType);
                if (id == null)
                {
                    Debug.LogWarning($"fold hierarchy : can't convert {sc.handle.GetType()} to {idType}");
                    return;
                }

                foreach (Object w in windows)
                {
                    method.Invoke(w, new object[] { id, false });
                }
            }
        }

        /// <summary>
        /// scene.handle => hierarchy tree item id
        /// handle is int or SceneHandle, id is int or EntityId, depending on unity version
        /// </summary>
        static object sceneToTreeId(Scene scene, System.Type idType)
        {
            object handle = scene.handle;
            System.Type handleType = handle.GetType();

            if (idType.IsAssignableFrom(handleType)) return handle;

            // SceneHandle.ToEntityId()
            var toId = handleType.GetMethod("ToEntityId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, System.Type.EmptyTypes, null);
            if (toId != null && toId.ReturnType == idType) return toId.Invoke(handle, null);

            // conversion operators (int/SceneHandle => EntityId)
            object id = convertWith(idType, handle, idType);
            if (id != null) return id;

            id = convertWith(handleType, handle, idType);
            if (id != null) return id;

            // ToEntityId as extension method (static classes)
            foreach (var asm in new[] { idType.Assembly, typeof(Scene).Assembly })
            {
                foreach (var type in getTypes(asm))
                {
                    if (!type.IsAbstract || !type.IsSealed) continue;

                    id = convertWith(type, handle, idType);
                    if (id != null) return id;
                }
            }

            return null;
        }

        /// <summary>
        /// search in owner a static (handle) => id method
        /// </summary>
        static object convertWith(System.Type owner, object handle, System.Type idType)
        {
            foreach (var m in owner.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (m.Name != "op_Implicit" && m.Name != "op_Explicit" && m.Name != "ToEntityId") continue;
                if (m.ReturnType != idType) continue;

                var ps = m.GetParameters();
                if (ps.Length == 1 && ps[0].ParameterType == handle.GetType())
                    return m.Invoke(null, new[] { handle });
            }
            return null;
        }

        static System.Type[] getTypes(Assembly asm)
        {
            try { return asm.GetTypes(); }
            catch (ReflectionTypeLoadException e) { return System.Array.FindAll(e.Types, x => x != null); }
        }
    }
}