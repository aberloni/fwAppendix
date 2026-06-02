using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace fwp.scenes
{

    public class SceneUnloader : MonoBehaviour
    {
        static public bool Verbose = SceneLoader.verbose;


        static public void unloadScene(Scene sc)
        {
            unloadScene(sc.name);
        }

        static public void unloadScene(string nm, Action onComplete = null)
        {
            if (nm.Length <= 0)
            {
                Debug.LogWarning("given name is empty, nothing to unload ?");
                onComplete();
                return;
            }

            unloadScenes(new string[] { nm }, onComplete);
        }

        static public void unloadScenes(string[] nms, Action onComplete = null)
        {
            SceneLoaderRunner.createLoader().asyncUnloadScenes(nms, onComplete);
        }

        static public void unloadScenesInstant(Scene sc) { unloadScenesInstant(new string[] { sc.name }); }
        static public void unloadScenesInstant(string[] nms)
        {
            for (int i = 0; i < nms.Length; i++)
            {
                Scene sc = SceneLoader.getLoadedScene(nms[i], true);
                if (sc.isLoaded)
                {
                    if (Verbose) Debug.Log("unloading : " + sc.name);

                    SceneManager.UnloadSceneAsync(nms[i]);
                }
            }
        }

    }

}