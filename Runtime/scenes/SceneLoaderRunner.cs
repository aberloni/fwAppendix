using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using System;

namespace fwp.scenes
{
    /// <summary>
    /// hierarchy object to track load of scenes
    /// </summary>
    public class SceneLoaderRunner : MonoBehaviour
    {
        SceneAssoc[] assocs;

        protected List<Coroutine> queries = new List<Coroutine>();

        static public SceneLoaderRunner createLoader()
        {
            GameObject go = new GameObject("[loader(" + UnityEngine.Random.Range(0, 1000) + ")]");

            DontDestroyOnLoad(go);

            return go.AddComponent<SceneLoaderRunner>();
        }

        private void Awake()
        {
            if (SceneLoader.verbose)
                Debug.Log("sceneloader:loader created:" + name, this);

            //Debug.Log(EngineObject.getStamp(this) + " created");
            //SceneLoader.loaders.Add(this);
        }

        private void OnDestroy()
        {
            //Debug.Log(EngineObject.getStamp(this) + " destroyed");
            //SceneLoader.loaders.Remove(this);
        }

        public Coroutine asyncLoadScenes(string[] sceneNames, Action<SceneAssoc[]> onComplete = null, float delayOnCompletion = 0f)
        {
            assocs = SceneAssoc.solveScenesAssocs(sceneNames, true);

            return StartCoroutine(processLoadScenes(onComplete, delayOnCompletion));
        }

        IEnumerator processLoadScenes(Action<SceneAssoc[]> onComplete = null, float delayOnCompletion = 0f)
        {
            SceneLoader.log(" ... processing " + assocs.Length + " scenes", transform);

            // WAIT FOR ENGINE VALIDITY
            if (Time.frameCount <= 1)
            {
                SceneLoader.log(" ... waiting for frame 2 ...", this);

                //unity flags scenes as loaded after frame 1
                //need to wait for when the scene is already present
                while (Time.frameCount <= 2)
                {
                    SceneLoader.log($" @frame {Time.frameCount}", this);

                    yield return null;
                }
            }

            for (int i = 0; i < assocs.Length; i++)
            {
                //Debug.Log(getStamp() + " what about ? " + sceneName, this);

                if(!assocs[i].handle.isLoaded)
                {
                    StartCoroutine(processLoadScene(assocs[i]));
                }

            }

            int cnt = countStillLoading();
            if(cnt > 0)
            {
                SceneLoader.log("    now waiting for x" + cnt + " scenes to be loaded");

                while (cnt > 0)
                {
                    int _cnt = countStillLoading();
                    if(_cnt != cnt)
                    {
                        cnt = _cnt;
                        SceneLoader.log("    ... still loading x" + _cnt);
                    }
                    yield return null;
                }

            }
            
            if (SceneLoader.verbose)
            {
                SceneLoader.log("is <b>done loading</b>", this);

                for (int i = 0; i < assocs.Length; i++)
                {
                    Debug.Log("#" + i + " ? " + assocs[i]);
                }
            }

            //needed so that all new objects loaded have time to exec build()
            //ca fait un effet de bord quand on unload le screen dans la frame où il est généré
            //la callback de sortie du EngineLoader peut demander l'écran qu'on vient de load pour en faire un truc :shrug:
            //avant qu'on setup des trucs dans l'écran faut que tlm ai fait son build
            yield return null;

            // create a arbitrary delay in loading
            if (delayOnCompletion > 0f)
            {
                SceneLoader.log("DELAY " + delayOnCompletion);

                while (delayOnCompletion > 0f)
                {
                    delayOnCompletion -= Time.deltaTime;
                    yield return null;
                }
            }

            // callback result
            if (onComplete != null) onComplete(assocs);

            // remove loader
            GameObject.Destroy(gameObject);
        }

        int countStillLoading()
        {

            int cnt = 0;
            for (int i = 0; i < assocs.Length; i++)
            {
                if (!assocs[i].handle.isLoaded)
                    cnt++;
            }

            return cnt;
        }

        /// <summary>
        /// process to load an instance
        /// </summary>
        IEnumerator processLoadScene(SceneAssoc assoc, Action<SceneAssoc> onComplete = null)
        {
            string target = assoc.name;

            SceneLoader.log("  L <b>" + target + "</b> loading ... ");

            AsyncOperation async = SceneManager.LoadSceneAsync(target, LoadSceneMode.Additive);
            while (!async.isDone)
            {
                yield return null;
                //Debug.Log(sceneLoad + " "+async.progress);
            }

            SceneLoader.log("  L <b>" + target + "</b> async is done ... ");

            Scene sc = SceneManager.GetSceneByName(target);
            
            if(!sc.isLoaded)
            {
                while (!sc.isLoaded) yield return null;
            }

            assoc.handle = sc;

            SceneLoader.log("  L <b>" + target + "</b> is loaded");

            // clean guides
            SceneLoader.cleanScene(sc);

            // feeders of this scene
            Coroutine feeders = solveFeeders(sc, delegate ()
            {
                feeders = null;
            });
            while (feeders != null) yield return null;

            SceneLoader.log("'<b>" + target + "</b>' setup");

            if (onComplete != null) onComplete(assoc);
        }



        //


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


        //


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

                SceneLoader.log("  now unloading {sceneNames[i]} ...");

                AsyncOperation async = SceneManager.UnloadSceneAsync(sceneNames[i]);
                if (async == null)
                {
                    Debug.LogWarning("no asyncs returned for scene of name " + sceneNames[i]);
                    continue;
                }

                asyncsToUnload.Add(async);
            }

            SceneLoader.log("unloading scenes x" + sceneNames.Length + ", asyncs x" + asyncsToUnload.Count);

            //wait for all
            while (asyncsToUnload.Count > 0)
            {
                while (!asyncsToUnload[0].isDone) yield return null;
                asyncsToUnload.RemoveAt(0);
                yield return null;
            }

            if (onCompletionDelay > 0f)
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

    }

}
