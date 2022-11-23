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


        static public List<string> getScenesNamesOfCategory(string cat)
        {
            List<string> output = new List<string>();

            //string[] scenes = HalperScene.getAllBuildSettingsScenes(false);
            string[] scenes = HalperScene.getAssetScenesPaths();

            if (scenes.Length <= 0)
            {
                Debug.LogWarning("no scenes ?");
                return output;
            }

            for (int i = 0; i < scenes.Length; i++)
            {
                string path = scenes[i].ToLower();

                if (path.Contains("/3rd")) continue;

                if (!path.Contains(cat.ToLower())) continue;

                string scName = scenes[i].Substring(scenes[i].LastIndexOf("/") + 1);

                if (scName.EndsWith(".unity")) scName = scName.Substring(0, scName.IndexOf(".unity"));

                output.Add(scName);
            }

            //Debug.Log($"found x{regionScenes.Count} regions");

            return output;
        }

    }

}
