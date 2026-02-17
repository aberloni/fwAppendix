using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace fwp.scenes
{
    static public class SceneTools
    {

        const char slash = '/';
        const string sceneExt = ".unity";

        static public bool isDebugScene()
        {
            if (isSceneOfType("test-")) return true;
            if (isSceneOfType("debug-")) return true;
            return false;
        }

        static public bool isTestScene()
        {
            if (isSceneOfType("test-")) return true;
            return false;
        }

        static public string getLevelName() { return SceneManager.GetActiveScene().name; }
        static public bool isSceneOfType(string nm) { return getLevelName().StartsWith(nm); }
        static public bool isSceneOfName(string nm) { return getLevelName().Contains(nm); }

        /* remove everything with a * at the start of the name */
        static public void removeGuides()
        {
            GameObject[] objs = fwp.appendix.AppendixUtils.gcs<GameObject>();
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].name[0] == '*') GameObject.Destroy(objs[i].gameObject);
            }
        }

        /// <summary>
        /// will keep anything after last /
        /// </summary>
        static public string removePathBeforeFile(string path)
        {
            return path.Substring(path.LastIndexOf(slash) + 1);
        }

        static public string removeUnityExt(string path)
        {
            if (path.EndsWith(sceneExt)) path = path.Substring(0, path.IndexOf(sceneExt));
            return path;
        }

        /// <summary>
        /// returns path relative to unity project (starts with assets/)
        /// remove sys to remove part of the path outside of unity
        /// </summary>
        static public List<string> getScenesPathsOfCategory(string folderContains, bool removeExt = false)
        {
            string[] scenes = new string[0];

            //if (!folderContains.EndsWith("/")) folderContains += "/";
            if (!folderContains.StartsWith(slash)) folderContains = slash + folderContains;
            folderContains = folderContains.ToLower();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // !runtime : asset database
                scenes = getProjectAssetScenesPaths(); // uses buff filtering
            }
#endif

            if (scenes.Length <= 0)
            {
                // @runtime / @build, use build settings list instead
                scenes = getAllBuildSettingsScenes(false);
            }

            List<string> output = new List<string>();

            if (scenes.Length <= 0)
            {
                Debug.LogWarning("no scenes ?");
                return output;
            }

            for (int i = 0; i < scenes.Length; i++)
            {
                string path = scenes[i];

                //Debug.Log(pathLower + " vs " + folderContains);
                if (!path.Contains(folderContains)) continue;

                if (removeExt) path = removeUnityExt(path);
                output.Add(path);
            }

            //Debug.Log($"found x{regionScenes.Count} regions");

            return output;
        }

#if UNITY_EDITOR
        /// <summary>
        /// only returns scene names
        /// </summary>
        static public List<string> getScenesNamesOfCategory(string cat)
        {
            List<string> paths = getScenesPathsOfCategory(cat, true);
            List<string> output = new List<string>();

            foreach (string path in paths)
            {
                // remove path
                string scName = removePathBeforeFile(path);

                // remove ext, jic
                //scName = removeUnityExt(scName);

                output.Add(scName);
            }

            //Debug.Log($"found x{regionScenes.Count} regions");

            return output;
        }
#endif

        static public bool isRuntimeSceneLoaded(string sceneName)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
            return scene.isLoaded;
        }

#if UNITY_EDITOR
        static public bool isEditorSceneLoaded(string sceneName)
        {
            var scene = getEditorSceneLoaded(sceneName);
            return scene != null;
        }

        static public UnityEngine.SceneManagement.Scene? getEditorSceneLoaded(string sceneName)
        {
            var scene = EditorSceneManager.GetSceneByName(sceneName);
            if (scene.IsValid() && scene.isLoaded)
            {
                return scene;
            }
            return null;
        }

#endif


        static public bool checkIfCanBeLoaded(string sceneName)
        {
            string[] all = getAllBuildSettingsScenes(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].Contains(sceneName)) return true;
            }
            return false;
        }

        static public bool isSceneOpened(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene sc = SceneManager.GetSceneAt(i);
                if (sc.name == sceneName) return true;
            }
            return false;
        }

        static public string[] getAllBuildSettingsScenes(bool removePath)
        {
            List<string> paths = new List<string>();

            //Debug.Log(SceneManager.sceneCountInBuildSettings);


            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);

                if (removePath)
                {
                    int slashIndex = path.LastIndexOf(slash);

                    if (slashIndex >= 0)
                    {
                        path = path.Substring(slashIndex + 1);
                    }

                    path = path.Remove(path.LastIndexOf(sceneExt));

                }

                paths.Add(path);
            }

            return paths.ToArray();
        }


#if UNITY_EDITOR

        static public string getSceneAssetFullPath(string sceneName, bool contains = true)
        {
            // in project t:Scene
            string[] paths = getProjectAssetScenesPaths();

            // more optimized way !contains comparison
            sceneName = sceneName += sceneExt;

            for (int i = 0; i < paths.Length; i++)
            {
                //  path/to/scene.unity
                if (!paths[i].Contains(sceneExt)) continue;

                if (contains && paths[i].Contains(sceneName)) return paths[i];
                else
                {
                    // rem .unity
                    //var path = paths[i].Substring(0, paths[i].IndexOf("."));

                    if (paths[i].EndsWith(sceneName)) return paths[i];
                }
            }

            return string.Empty;
        }

        static public string getBuildSettingsFullPathOfScene(string partName)
        {
            string fullName = getBuildSettingsSceneFullName(partName);
            string[] paths = getAllBuildSettingsScenes(false);
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].Contains(fullName))
                {
                    return paths[i];
                }
            }

            return string.Empty;
        }


        static public void addSceneToBuildSettings(string sceneName)
        {
            if (isSceneInBuildSettings(sceneName, true)) return;

            string assetPath = getSceneAssetFullPath(sceneName);

            //string fullName = getBuildSettingsSceneFullName(sceneName);

            List<EditorBuildSettingsScene> all = new List<EditorBuildSettingsScene>();
            all.AddRange(EditorBuildSettings.scenes);

            //string path = getBuildSettingsFullPathOfScene(sceneName);

            EditorBuildSettingsScene addScene = new EditorBuildSettingsScene(assetPath, true);
            all.Add(addScene);

            EditorBuildSettings.scenes = all.ToArray();
        }


        static public string getPathOfSceneInProject(string sceneName)
        {
            string[] paths = getProjectAssetScenesPaths();

            for (int i = 0; i < paths.Length; i++)
            {
                // Assets/Modules/module-a-b.unity
                string path = paths[i];

                string pathSceneName = path.Substring(0, path.LastIndexOf("."));
                pathSceneName = pathSceneName.Substring(pathSceneName.LastIndexOf(slash) + 1);

                // module-a-b
                //Debug.Log(pathSceneName);

                if (pathSceneName == sceneName) return path;
            }
            return string.Empty;
        }


        static public bool isSceneInBuildSettings(string partName, bool hardCheck = false)
        {

            string nm = getBuildSettingsSceneFullName(partName);
            if (nm.Length < 0) return false;

            if (hardCheck) return nm == partName;
            return true;
        }

        static public string getBuildSettingsSceneFullName(string partName)
        {
            if (partName.EndsWith(sceneExt)) partName = partName.Substring(0, partName.IndexOf(sceneExt));

            string[] all = getAllBuildSettingsScenes(true); // no path
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].Contains(partName))
                {
                    return all[i];
                }
            }
            return string.Empty;
        }

        static string[] __scene_paths;

        static public string[] getProjectAssetScenesPaths()
        {
            if (__scene_paths == null) solveProjectAssetScenesPaths();
            return __scene_paths;
        }

        /// <summary>
        /// fetch all scene present in database
        /// this should return all scene in projet
        /// all paths are lowercased
        /// </summary>
        static public void solveProjectAssetScenesPaths()
        {
            Debug.LogWarning("/! refresh Scene[]");
            var paths = AssetDatabase.FindAssets("t:Scene");

            if (paths.Length <= 0)
            {
                Debug.LogWarning("asking for scene but none ?");
                __scene_paths = new string[0];
                return;
            }

            Debug.LogWarning("/! replacing GUID x" + paths.Length);

            //replace GUID by full path
            for (int i = 0; i < paths.Length; i++)
            {
                paths[i] = AssetDatabase.GUIDToAssetPath(paths[i]).ToLower();
            }

            __scene_paths = paths;
        }

#endif

    }

}
