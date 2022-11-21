using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
//using UnityEditor.SceneManagement;
using UnityEditor;
#endif
/// <summary>
/// USed to declare specific additionnal scene to load on startup
/// </summary>

namespace fwp.scenes
{
    public class SceneLoaderFeeder : SceneLoaderFeederBase
    {
        public bool add_camera;

        [Header("prefix resource-")]
        public string[] resource_names;

        [Header("prefix ui-")]
        public string[] ui_names; // ui element (overlay)

        [Header("prefix graphics-")]
        public string[] graphics_names; // objects (3d, sprite) ingame

        [Header("prefix screen-")]
        public string[] screens_names;

        [Header("no prefix")]
        public string[] other_names;

        /// <summary>
        /// generate list of scenes with exact names
        /// </summary>
        /// <returns></returns>
        override protected string[] solveNames()
        {
            base.solveNames();

            if (add_camera) addWithPrefix("resource-", "camera");

#if tracker
    addWithPrefix("resource-", "tracker");
#endif

            addWithPrefix("resource-", resource_names);
            addWithPrefix("ui-", ui_names);
            addWithPrefix("graphics-", graphics_names);
            addWithPrefix("screen-", screens_names);

            addWithPrefix("", other_names);

            //check si on doit garder l'objet qui porte les feeders
            MonoBehaviour[] monos = gameObject.GetComponents<MonoBehaviour>();
            if (monos.Length == 1 && monos[0] == this)
            {
                GameObject.Destroy(gameObject);
            }
            else
            {
                GameObject.Destroy(this);
            }

            return scene_names.ToArray();
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

        [ContextMenu("fetch graphics")]
        protected void fetchGraphics()
        {
            this.graphics_names = fetchScenesRefs("graphics");
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }

        [ContextMenu("fetch uis")]
        protected void fetchUis()
        {
            this.ui_names = fetchScenesRefs("ui");
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }

        [ContextMenu("fetch screens")]
        protected void fetchScreens()
        {
            this.screens_names = fetchScenesRefs("screen");
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }

#endif
    }

}
