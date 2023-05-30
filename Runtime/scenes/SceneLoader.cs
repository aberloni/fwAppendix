using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Random = UnityEngine.Random;

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

        static public List<SceneLoader> loaders = new List<SceneLoader>();

        protected List<Coroutine> queries = new List<Coroutine>();

        public const string prefixResource = "resource-";

        private void Awake()
        {
            if (verbose)
                Debug.Log("sceneloader:loader created:"+name, this);

            //Debug.Log(EngineObject.getStamp(this) + " created");
            loaders.Add(this);
        }

        private void OnDestroy()
        {
            //Debug.Log(EngineObject.getStamp(this) + " destroyed");
            loaders.Remove(this);
        }

        static protected SceneLoader createLoader()
        {
            GameObject go = new GameObject("[loader(" + Random.Range(0, 1000) + ")]");

            DontDestroyOnLoad(go);

            return go.AddComponent<SceneLoader>();
        }

        [System.Obsolete]
        static protected bool checkForFilteredScenes()
        {
            string[] filter = { "ui", "screen", "resource", "level" };
            for (int i = 0; i < filter.Length; i++)
            {
                if (doActiveSceneNameContains(filter[i]))
                {
                    //SceneManager.LoadScene("game");
                    Debug.LogWarning("<color=red><b>" + filter[i] + " SCENE ?!</b></color> can't load that");
                    return false;
                }
            }
            return true;
        }

        static public bool hasAnyScenesInBuildSettings()
        {

            if (SceneManager.sceneCountInBuildSettings <= 1)
            {
                Debug.LogWarning("could not launch loading because <b>build settings scenes list count <= 1</b>");
                return false;
            }

            return true;
        }

        public Coroutine solveFeeders(Scene scene, Action onComplete = null)
        {
            return StartCoroutine(processFeeders(scene, onComplete));
        }

        IEnumerator processFeeders(Scene scene, Action onFeedersCompleted = null)
        {
            ///// feeder, additionnal scenes (from feeder script)
            GameObject[] roots = scene.GetRootGameObjects();
            List<SceneLoaderFeederBase> feeders = new List<SceneLoaderFeederBase>();
            for (int i = 0; i < roots.Length; i++)
            {
                feeders.AddRange(roots[i].GetComponentsInChildren<SceneLoaderFeederBase>());
            }

            if (feeders.Count > 0)
            {
                //start
                for (int i = 0; i < feeders.Count; i++)
                {
                    feeders[i].feed();
                }

                Debug.Log("waiting for x" + feeders.Count + " feeders");

                bool done = false;
                while (!done)
                {
                    done = true;
                    for (int i = 0; i < feeders.Count; i++)
                    {
                        if (feeders[i] != null) done = false;
                    }
                    yield return null;
                }
            }

            onFeedersCompleted?.Invoke();
        }

        public Coroutine asyncUnloadScenes(string[] sceneNames, Action onComplete = null, float onCompletionDelay = 0f)
        {
            return StartCoroutine(processUnload(sceneNames, onComplete, onCompletionDelay));
        }

        IEnumerator processUnload(string[] sceneNames, Action onComplete = null, float onCompletionDelay = 0f)
        {
            List<AsyncOperation> asyncsToUnload = new List<AsyncOperation>();

            for (int i = 0; i < sceneNames.Length; i++)
            {
                Debug.Assert(sceneNames[i] != null, "string is null ?");
                Debug.Assert(sceneNames[i].Length > 0, "can't unload given empty scene name");

                Scene sc = SceneManager.GetSceneByName(sceneNames[i]);

                if (!sc.IsValid())
                {
                    Debug.LogWarning(sceneNames[i] + " not valid ?");
                    continue;
                }

                if (!sc.isLoaded)
                {
                    Debug.LogWarning(sceneNames[i] + " is not loaded ?");
                    continue;
                }

                Debug.Log($"  now unloading {sceneNames[i]} ...");

                AsyncOperation async = SceneManager.UnloadSceneAsync(sceneNames[i]);
                if (async == null)
                {
                    Debug.LogWarning("no asyncs returned for scene of name " + sceneNames[i]);
                    continue;
                }

                asyncsToUnload.Add(async);
            }

            Debug.Log(getStamp() + " unloading scenes x" + sceneNames.Length + ", asyncs x" + asyncsToUnload.Count);

            //wait for all
            while (asyncsToUnload.Count > 0)
            {
                while (!asyncsToUnload[0].isDone) yield return null;
                asyncsToUnload.RemoveAt(0);
                yield return null;
            }

            if(onCompletionDelay > 0f)
            {
                while (onCompletionDelay > 0f)
                {
                    onCompletionDelay -= Time.deltaTime;
                    yield return null;
                }

                yield return null;
            }
            
            if (onComplete != null) onComplete();

            GameObject.Destroy(gameObject);
        }

        public Coroutine asyncLoadScenes(string[] sceneNames, Action<Scene[]> onComplete = null, float delayOnCompletion = 0f)
        {
            return StartCoroutine(processLoadScenes(sceneNames, onComplete, delayOnCompletion));
        }

        IEnumerator processLoadScenes(string[] sceneNames, Action<Scene[]> onComplete = null, float delayOnCompletion = 0f)
        {
            if(verbose)
                Debug.Log(getStamp() + " ... processing " + sceneNames.Length + " scenes", transform);

            if(Time.frameCount < 2)
            {
                if (verbose)
                    Debug.Log(getStamp() + " ... waiting for frame 2 ...", this);

                //unity flags scenes as loaded after frame 1
                //need to wait for when the scene is already present
                while (Time.frameCount < 2)
                {
                    if (verbose)
                        Debug.Log(getStamp() + $" @frame {Time.frameCount}", this);

                    yield return null;
                }
            }


            if (verbose)
                Debug.Log(getStamp() + $" ... now filtering x{sceneNames.Length} scene names", this);

            List<string> filtered = new List<string>();

            List<Scene> output = new List<Scene>(); // list of all scene to return when done

            for (int i = 0; i < sceneNames.Length; i++)
            {
                string sceneName = sceneNames[i];

                //do not load the current active scene
                if (doActiveSceneNameContains(sceneName))
                {
                    Debug.LogWarning(sceneName + " is current active scene, need to load it ?");
                    output.Add(SceneManager.GetActiveScene());
                    continue;
                }

                //don't double load same scene
                var scene = getLoadedScene(sceneName);
                bool alreadyLoaded = scene.isLoaded;

                //Debug.Log(getStamp() + Time.frameCount + "  is " + sceneName + " <b>already loaded ?</b> "+ alreadyLoaded);

                if (alreadyLoaded)
                {
                    Debug.LogWarning("  <b>" + sceneName + "</b> is considered as already loaded, skipping loading of that scene");
                    output.Add(scene);
                    continue;
                }

                filtered.Add(sceneName);
            }

            if(verbose)
                Debug.Log(getStamp() + " filtered x" + filtered.Count + " out of given x" + sceneNames.Length);

            for (int i = 0; i < filtered.Count; i++)
            {
                string sceneName = filtered[i];

                //Debug.Log(getStamp() + " what about ? " + sceneName, this);

                StartCoroutine(processLoadScene(filtered[i], (Scene sc) =>
                {
                    output.Add(sc);

                    filtered.Remove(sc.name);

                    if(verbose)
                        Debug.Log(getStamp() + " scene : " + sc.name + " is done (remaining ? x" + filtered.Count + ")");

                }));

            }

            if(verbose)
                Debug.Log(getStamp() + " now waiting for x" + filtered.Count + " scenes to be loaded");

            int cnt = filtered.Count;
            while (filtered.Count > 0)
            {
                if(cnt != filtered.Count)
                {
                    cnt = filtered.Count;
                    Debug.Log(" ... remaining x" + cnt);
                }

                yield return null;
            }

            Debug.Log(getStamp() + " is <b>done loading</b> , output x"+output.Count, this);

            //needed so that all new objects loaded have time to exec build()
            //ca fait un effet de bord quand on unload le screen dans la frame où il est généré
            //la callback de sortie du EngineLoader peut demander l'écran qu'on vient de load pour en faire un truc :shrug:
            //avant qu'on setup des trucs dans l'écran faut que tlm ai fait son build
            yield return null;

            // create a arbitrary delay in loading
            if(delayOnCompletion > 0f)
            {
                while(delayOnCompletion > 0f)
                {
                    delayOnCompletion -= Time.deltaTime;
                    yield return null;
                }

                // just for show
                yield return null;
            }

            // callback result
            if (onComplete != null) onComplete(output.ToArray());

            // remove loader
            GameObject.Destroy(gameObject);
        }

        IEnumerator processLoadScene(string sceneLoad, Action<Scene> onComplete = null)
        {
            //can't reload same scene
            //if (isSceneOfName(sceneLoad)) yield break;

            name += "-" + sceneLoad;

            if (!checkIfInBuildSettings(sceneLoad))
            {
                Debug.LogWarning("asked to load <b>" + sceneLoad + "</b> but this scene is <color=red><b>not added to BuildSettings</b></color>");
                if (onComplete != null) onComplete(SceneManager.GetSceneByName(sceneLoad));
                yield break;
            }

            if (verbose)
                Debug.Log(getStamp() + "  L <b>" + sceneLoad + "</b> loading ... ");

            AsyncOperation async = SceneManager.LoadSceneAsync(sceneLoad, LoadSceneMode.Additive);
            while (!async.isDone)
            {
                yield return null;
                //Debug.Log(sceneLoad + " "+async.progress);
            }

            if(verbose)
                Debug.Log(getStamp() + "  L <b>" + sceneLoad + "</b> async is done ... ");

            Scene sc = SceneManager.GetSceneByName(sceneLoad);
            while (!sc.isLoaded) yield return null;
            if (verbose)
                Debug.Log(getStamp() + "  L <b>" + sceneLoad + "</b> at loaded state ... ");

            cleanScene(sc);

            Coroutine feeders = solveFeeders(sc, delegate ()
            {
                feeders = null;
            });
            while (feeders != null) yield return null;

            yield return null;

            //ResourceManager.reload(); // add resources if any

            if(verbose)
                Debug.Log(getStamp() + " ... '<b>" + sceneLoad + "</b>' loaded");

            yield return null;

            if (onComplete != null) onComplete(sc);
        }

        protected string getStamp()
        {
            return Time.frameCount + " " + GetType();
        }




        static public SceneLoader loadScene(string nm, Action<Scene> onComplete = null, float onCompletionDelay = 0f)
        {
            return loadScenes(new string[] { nm }, (Scene[] scs) =>
            {
                Debug.Assert(scs.Length > 0, "scenes array is empty ? "+nm);
                onComplete?.Invoke(scs[0]);
            }, onCompletionDelay);
        }
        static public SceneLoader loadScenes(string[] nms, Action<Scene[]> onComplete = null, float onCompletionDelay = 0f)
        {
            var loader = createLoader();
            loader.asyncLoadScenes(nms, onComplete, onCompletionDelay);
            return loader;
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
            createLoader().asyncUnloadScenes(nms, onComplete);
        }

        static public void unloadScenesInstant(Scene sc) { unloadScenesInstant(new string[] { sc.name }); }
        static public void unloadScenesInstant(string[] nms)
        {
            for (int i = 0; i < nms.Length; i++)
            {
                Scene sc = getLoadedScene(nms[i]);
                if (sc.isLoaded)
                {
                    if(verbose)
                        Debug.Log("unloading : " + sc.name);

                    SceneManager.UnloadSceneAsync(nms[i]);
                }
            }
        }

        static public Coroutine queryScene(string sceneName, Action<Scene> onComplete = null)
        {
            return queryScenes(new string[] { sceneName }, (Scene[] scs) =>
            {
                onComplete?.Invoke(scs[0]);
            });
        }
        static public Coroutine queryScenes(string[] sceneNames, Action<Scene[]> onComplete = null)
        {
            return createLoader().asyncLoadScenes(sceneNames, onComplete);
        }

        static public void unloadSceneByExactName(string sceneName)
        {
            if(verbose)
                Debug.Log("unloading <b>" + sceneName + "</b>");

            SceneManager.UnloadSceneAsync(sceneName);
        }

        static protected void cleanScene(Scene sc)
        {

            GameObject[] roots = sc.GetRootGameObjects();
            //Debug.Log("  L cleaning scene <b>" + sc.name + "</b> from guides objects (" + roots.Length + " roots)");
            for (int i = 0; i < roots.Length; i++)
            {
                removeGuides(roots[i].transform);
            }

        }

        static protected bool removeGuides(Transform obj)
        {
            if (obj.name.StartsWith("~"))
            {
                if(verbose)
                    Debug.Log("   <b>removing guide</b> of name : " + obj.name, obj);

                GameObject.Destroy(obj.gameObject);
                return true;
            }

            int i = 0;
            while (i < obj.childCount)
            {
                if (!removeGuides(obj.GetChild(i))) i++;
            }

            return false;
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
        static public Scene getLoadedScene(string strictSceneName)
        {
            if (Time.frameCount < 2)
            {
                Debug.LogError($"asking for scene {strictSceneName} but scenes are not flagged as loaded until frame 2");
                return default(Scene);
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
