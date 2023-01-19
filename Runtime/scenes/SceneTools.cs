using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace fwp.scenes
{
    static public class SceneTools
    {

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
            GameObject[] objs = GameObject.FindObjectsOfType<GameObject>();
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].name[0] == '*') GameObject.Destroy(objs[i].gameObject);
            }
        }

        /// <summary>
        /// returns path relative to unity project (starts with assets/)
        /// remove sys to remove part of the path outside of unity
        /// </summary>
        static public List<string> getScenesPathsOfCategory(string cat, bool removeExt = true)
        {
            List<string> output = new List<string>();

            //string[] scenes = HalperScene.getAllBuildSettingsScenes(false);
            string[] scenes = appendix.AppendixUtils.getAssetScenesPaths();

            if (scenes.Length <= 0)
            {
                Debug.LogWarning("no scenes ?");
                return output;
            }

            for (int i = 0; i < scenes.Length; i++)
            {
                string pathLower = scenes[i].ToLower();

                if (pathLower.Contains("/3rd")) continue;

                if (!pathLower.Contains(cat.ToLower())) continue;

                string path = scenes[i];
                if (removeExt) path = removeUnityExt(path);
                output.Add(path);
            }

            //Debug.Log($"found x{regionScenes.Count} regions");

            return output;
        }

        static public string removePathBeforeFile(string path)
        {
            return path.Substring(path.LastIndexOf("/") + 1);
        }

        static public string removeUnityExt(string path)
        {
            if (path.EndsWith(".unity")) path = path.Substring(0, path.IndexOf(".unity"));
            return path;
        }

        /// <summary>
        /// only returns scene names
        /// </summary>
        static public List<string> getScenesNamesOfCategory(string cat)
        {
            List<string> paths = getScenesPathsOfCategory(cat);
            List<string> output = new List<string>();

            foreach(string path in paths)
            {
                // remove path
                string scName = removePathBeforeFile(path);

                // remove ext, jic
                scName = removeUnityExt(scName);
                output.Add(scName);
            }

            //Debug.Log($"found x{regionScenes.Count} regions");

            return output;
        }

        static public bool isSceneLoaded(string sceneName)
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByName(sceneName);
            return scene.isLoaded;
        }

    }

}
