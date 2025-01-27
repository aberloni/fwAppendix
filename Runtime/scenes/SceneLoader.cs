using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Un loader indépendant a qui on peut demander de load une scene spécifique
/// Quand la scene sera load il va process les feeders lié a chaques scenes demandés
/// Puis se détruire
/// </summary>

namespace fwp.scenes
{
    public class SceneLoader : MonoBehaviour
    {
        static public bool verbose = false;

        static public List<SceneAssoc> loaders = new List<SceneAssoc>();

        public const string prefixResource = "resource-";

        static public bool hasAnyScenesInBuildSettings()
        {

            if (SceneManager.sceneCountInBuildSettings <= 1)
            {
                Debug.LogWarning("could not launch loading because <b>build settings scenes list count <= 1</b>");
                return false;
            }

            return true;
        }


        static public void log(string content, UnityEngine.Object context = null)
        {
            if (!verbose)
                return;

            Debug.Log(getStamp() + content, context);
        }

        static public string getStamp()
        {
            return Time.frameCount + "@SceneLoader| ";
        }

        static public SceneLoaderRunner loadScenes(string[] nms, Action<SceneAssoc[]> onComplete = null, float onCompletionDelay = 0f)
        {
            var loader = SceneLoaderRunner.createLoader();
            loader.asyncLoadScenes(nms, onComplete, onCompletionDelay);
            return loader;
        }

        static public SceneLoaderRunner loadScene(string nm, Action<SceneAssoc> onComplete = null, float onCompletionDelay = 0f)
        {
            // only one
            return loadScenes(new string[] { nm }, 
                (SceneAssoc[] scs) =>
                {
                    onComplete?.Invoke(scs[0]);
                }, onCompletionDelay);
        }

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
                Scene sc = getLoadedScene(nms[i], true);
                if (sc.isLoaded)
                {
                    if(verbose)
                        Debug.Log("unloading : " + sc.name);

                    SceneManager.UnloadSceneAsync(nms[i]);
                }
            }
        }

        static public Coroutine queryScene(string sceneName, Action<SceneAssoc> onComplete = null)
        {
            return queryScenes(new string[] { sceneName }, (SceneAssoc[] scs) =>
            {
                SceneAssoc sa = null;

                if (scs != null && scs.Length > 0) sa = scs[0];
                
                onComplete?.Invoke(sa);
            });
        }
        static public Coroutine queryScenes(string[] sceneNames, Action<SceneAssoc[]> onComplete = null)
        {
            return SceneLoaderRunner.createLoader().asyncLoadScenes(sceneNames, onComplete);
        }

        static public void unloadSceneByExactName(string sceneName)
        {
            if(verbose)
                Debug.Log("unloading <b>" + sceneName + "</b>");

            SceneManager.UnloadSceneAsync(sceneName);
        }

        static private string getActiveSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        static public bool doActiveSceneNameContains(string nm, bool startWith = false)
        {
            string scName = getActiveSceneName();
            //Debug.Log(scName + " vs " + nm);
            if (startWith) return scName.StartsWith(nm);
            return scName.Contains(nm);
        }

        static public bool isGameScene()
        {
            return getActiveSceneName().StartsWith("game");
            //return doActiveSceneNameContains("game");
        }

        static protected bool isResourceScene()
        {
            return doActiveSceneNameContains("resource-");
        }

        static protected bool isSceneLevel()
        {
            return doActiveSceneNameContains("level-");
        }

        static public bool areAnyLoadersRunning()
        {
            return loaders.Count > 0;
        }

        /// <summary>
        /// loaded or loading (but called to be loaded at least)
        /// </summary>
        /// <param name="endName"></param>
        /// <returns></returns>
        static public bool isSceneAdded(string endName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene sc = SceneManager.GetSceneAt(i);
                //Debug.Log(sc.name + " , valid ? " + sc.IsValid() + " , loaded ? " + sc.isLoaded);
                if (sc.name.Contains(endName))
                {
                    return true;
                }
            }

            return false;
        }

        static public bool isScenePresent(string endName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene sc = SceneManager.GetSceneAt(i);
                if (!sc.IsValid()) continue;

                if (sc.name.EndsWith(endName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// this needs to be a strick comparison
        /// (use to check for already loaded scenes)
        /// </summary>
        static public Scene getLoadedScene(string strictSceneName, bool warnMissing)
        {
            if(Application.isPlaying)
            {
                if (Time.frameCount < 2)
                {
                    Debug.LogError($"asking for scene {strictSceneName} but scenes are not flagged as loaded until frame 2");
                    return default(Scene);
                }
            }
            
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene sc = SceneManager.GetSceneAt(i);

                //Debug.Log(sc.name + " , valid ? " + sc.IsValid() + " , loaded ? " + sc.isLoaded);

                //if (sc.isLoaded && sc.name.Contains(containName)) return sc;
                if (sc.isLoaded && sc.name == strictSceneName) return sc;
            }

            //Debug.LogWarning("asking if "+containName + " scene is loaded but its not");
            return default(Scene);
        }

        static public Scene? getLoadingScene(string strictSceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene sc = SceneManager.GetSceneAt(i);

                if (sc.isLoaded) continue;

                if (sc.name == strictSceneName) return sc;
            }

            return null;
        }

#if UNITY_EDITOR

        static public bool isSceneInBuildSettingsList(string scName)
        {
            bool found = true;

            found = false;

            UnityEditor.EditorBuildSettingsScene[] scenes = UnityEditor.EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                //UnityEditor.SceneManagement.EditorSceneManager.GetSceneByBuildIndex()
                if (scenes[i].path.Contains(scName)) found = true;
            }

            return found;
        }

#endif

        static public bool checkIfInBuildSettings(string sceneLoad)
        {
            bool checkIfExists = false;

            //Debug.Log("count ? "+ SceneManager.sceneCountInBuildSettings);

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);

                if (path.Contains(sceneLoad)) checkIfExists = true;

                //Debug.Log(path);
            }

            return checkIfExists;
        }

    }

}
