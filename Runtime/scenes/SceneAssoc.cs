using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace fwp.scenes
{

    /// <summary>
    /// meant to wrap load state of a scene
    /// used during load & in profils
    /// </summary>
    public class SceneAssoc
    {
        public string name;
        public Scene handle;
        public SceneLoaderRunner runner;

        public void setup(Scene refScene)
        {
            handle = refScene;
            name = handle.name;
            runner = null;
        }

        public bool isLoaded()
        {
            if (handle.IsValid() && handle.isLoaded) return true;
            return false;
        }



        /// <summary>
        /// fetch all assocs from given names
        /// called right before calling the actual loading
        /// </summary>
        static public SceneAssoc[] solveScenesAssocs(string[] sceneNames, bool warnMissing = false)
        {
            SceneLoader.log($" ... now filtering x{sceneNames.Length} scene names");

            List<SceneAssoc> output = new List<SceneAssoc>();

            for (int i = 0; i < sceneNames.Length; i++)
            {
                string sceneName = sceneNames[i];

                bool inBSettings = SceneLoader.checkIfInBuildSettings(sceneName);

                // impossible to load
                // not in bsettings
                if (!inBSettings)
                {
                    if (warnMissing)
                        Debug.LogWarning("asked to find scene:<b>" + sceneName + "</b> but this scene is <color=red><b>not added to BuildSettings</b></color>");

                    continue;
                }

                /*
                // is current active scene
                if (doActiveSceneNameContains(sceneName))
                {
                    Debug.LogWarning(sceneName + " is current active scene, need to load it ?");
                    output.Add(new SceneAssoc() { name = sceneName, handle = SceneManager.GetActiveScene() });
                    continue;
                }
                */

                SceneAssoc assoc = new SceneAssoc();
                assoc.name = sceneName;

                // don't double load same scene
                // check from already present scenes
                var scene = SceneLoader.getLoadedScene(sceneName, warnMissing);
                if (scene.isLoaded)
                {
                    if(warnMissing)
                        Debug.LogWarning("  <b>" + sceneName + "</b> is considered as already loaded, skipping loading of that scene");

                    assoc.handle = scene;
                }

                output.Add(assoc);
            }

            SceneLoader.log("filtered x" + output.Count + " out of given x" + sceneNames.Length);

            return output.ToArray();
        }


    }

}