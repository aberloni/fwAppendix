using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;
using System.Collections.Generic;

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
    }
}