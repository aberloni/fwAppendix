using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// USed to declare specific additionnal scene to load on startup
/// </summary>

namespace fwp.scenes.feeder
{
    /// <summary>
    /// multiple categories
    /// </summary>
    public class SceneLoaderFeeder : SceneLoaderFeederBase
    {
        [SerializeField]
        FeederData[] datas;

        /// <summary>
        /// generate list of scenes with exact names
        /// </summary>
        /// <returns></returns>
        override protected void solveNames()
        {
            if (datas != null)
            {
                foreach (var d in datas)
                {
                    addFeederData(d);
                }
            }
        }

#if UNITY_EDITOR

        protected string[] fetchScenesRefs(string type)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

            List<string> screens = new List<string>();
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                EditorBuildSettingsScene data = EditorBuildSettings.scenes[i];
                if (data.path.Contains(type + "-"))
                {
                    string[] split = data.path.Split('-'); // screen-xxx
                    split = split[split.Length - 1].Split('.'); // remove .asset
                    Debug.Log("adding " + split[0]);
                    screens.Add(split[0]);
                }
            }

            return screens.ToArray();
        }

#endif
    }

}
