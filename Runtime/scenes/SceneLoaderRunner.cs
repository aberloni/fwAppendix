using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using System;

namespace fwp.scenes
{
    /// <summary>
    /// hierarchy object to track load of scenes
    /// 
    /// smooth loading thread : https://gamedev.stackexchange.com/questions/130180/smooth-loading-screen-between-scenes
    ///     If you want the scene activation to not freeze your game, then you should keep your 
    ///     Awake and Start callbacks to a minimun, and initialize your scripts in a coroutine through several cycles.
    ///     
    /// loading thread priority : https://docs.unity3d.com/ScriptReference/Application-backgroundLoadingPriority.html
    /// 
    /// </summary>
    public class SceneLoaderRunner : MonoBehaviour
    {
        /// <summary>
        /// to modify behavior of loading
        /// </summary>
        public RunnerSettings settings;
        
        public class RunnerSettings
        {
            /// <summary>
            /// time before triggering allowSceneActivation
            /// if >0 will delay trigger
            /// </summary>
            public float delay_scene_activation;

            /// <summary>
            /// delay between each scene
            /// </summary>
            public float delayEach;

            /// <summary>
            /// delay between order group
            /// </summary>
            public float delayEachGroup;

            /// <summary>
            /// wait for framerate to be stable
            /// </summary>
            public bool stable_framerate;

            /// <summary>
            /// wait a bit after loading
            /// </summary>
            public float delayOnCompletion;
        }

        /*
        /// <summary>
        /// to display some info if a loading takes too long
        /// - when async is done but scene is not loaded
        /// </summary>
        static float errorDelay = -1f;
        static public void optinErrorDelay(float delay) => errorDelay = delay;
        static public void optoutErrorDelay() => errorDelay = -1f;
        */

        /// <summary>
        /// all element loaded during this process
        /// </summary>
        public SceneTargetLoader[] assocs;

        /// <summary>
        /// when process completes
        /// </summary>
        public System.Action<SceneTargetLoader[]> onCompletion;

        protected List<Coroutine> queries = new List<Coroutine>();

        static public SceneLoaderRunner createLoader()
        {
            GameObject go = new GameObject("[loader(" + UnityEngine.Random.Range(0, 1000) + ")]");
            DontDestroyOnLoad(go);

            return go.AddComponent<SceneLoaderRunner>();
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);

            if (SceneLoader.verbose) Debug.Log("sceneloader:loader created:" + name, this);
        }

        private void OnDestroy()
        {
            onCompletion?.Invoke(assocs);
            
            //Debug.Log(EngineObject.getStamp(this) + " destroyed");
            //SceneLoader.loaders.Remove(this);
        }

        public SceneLoaderRunner coroLoadScenes(string[] sceneNames)
        {
            assocs = SceneTargetLoader.solveScenesAssocs(sceneNames, true);
            StartCoroutine(dequeueScenes());
            return this;
        }

        IEnumerator dequeueScenes()
        {
            if (SceneLoader.verbose) SceneLoader.log(" ... processing " + assocs.Length + " scenes", transform);

            // WAIT FOR uENGINE VALIDITY
            if (Time.frameCount <= 1)
            {
                if (SceneLoader.verbose) SceneLoader.log(" ... waiting for frame 2 ...", this);

                //unity flags scenes as loaded after frame 1
                //need to wait for when the scene is already present
                while (Time.frameCount <= 2)
                {
                    if (SceneLoader.verbose) SceneLoader.log($" @frame {Time.frameCount}", this);

                    yield return null;
                }
            }

            //delay to leave time for settings assign
            yield return null;

            for (int i = 0; i < assocs.Length; i++)
            {
                //Debug.Log(getStamp() + " what about ? " + sceneName, this);

                if (!assocs[i].handle.isLoaded)
                {
                    StartCoroutine(dequeueScene(assocs[i], settings));
                }
            }

            int cnt = countStillLoading();
            if (cnt > 0)
            {
                if (SceneLoader.verbose) SceneLoader.log("    now waiting for x" + cnt + " scenes to be loaded");

                while (cnt > 0)
                {
                    int _cnt = countStillLoading();
                    if (_cnt != cnt)
                    {
                        cnt = _cnt;
                        if (SceneLoader.verbose) SceneLoader.log("    ... still loading x" + _cnt);
                    }
                    yield return null;
                }

            }

            //needed so that all new objects loaded have time to exec build()
            //ca fait un effet de bord quand on unload le screen dans la frame o� il est g�n�r�
            //la callback de sortie du EngineLoader peut demander l'�cran qu'on vient de load pour en faire un truc :shrug:
            //avant qu'on setup des trucs dans l'�cran faut que tlm ai fait son build
            yield return null;

            // create a arbitrary delay in loading
            if (settings != null && settings.delayOnCompletion > 0f)
            {
                // lock loading until framerate is stable
                yield return new WaitUntil(() => Time.deltaTime < 0.1f);

                if (SceneLoader.verbose) SceneLoader.log("DELAY " + settings.delayOnCompletion);
                yield return new WaitForSeconds(settings.delayOnCompletion);
            }

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
        IEnumerator dequeueScene(SceneTargetLoader assoc, RunnerSettings settings = null)
        {
            string target = assoc.name;

            float time = Time.unscaledTime;
            if (SceneLoader.verbose) SceneLoader.log("  L <b>" + target + "</b> loading ... @time:" + time);

            // https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html
            // https://docs.unity3d.com/ScriptReference/AsyncOperation.html
            AsyncOperation async = SceneManager.LoadSceneAsync(target, LoadSceneMode.Additive);

            if(settings != null && settings.delay_scene_activation > 0)
            {
                async.allowSceneActivation = settings.delay_scene_activation != 0f;
            }
            
            if (!async.allowSceneActivation)
            {
                //wait for loading to be done
                yield return new WaitUntil(() => async.progress >= 0.9f);

                float dt = Time.time - time;
                if (SceneLoader.verbose) SceneLoader.log(" ... delay activation @dt:" + dt);

                // give time after loading most of the scene
                yield return new WaitForSeconds(settings.delay_scene_activation);

                // this will trigger activation of scene
                async.allowSceneActivation = true;
            }

            yield return new WaitUntil(() => async.isDone);

            if (SceneLoader.verbose) SceneLoader.log("  L <b>" + target + "</b> async operation is done ... ");

            Scene sc = SceneManager.GetSceneByName(target);
            if (!sc.IsValid())
            {
                Debug.LogError("SceneManager could not return a valid scene by NAME : " + target);
                yield break;
            }

            if (!sc.isLoaded)
            {
                if (SceneLoader.verbose) SceneLoader.log(sc.name + " :   async is done but scene is not loaded ? waiting ...");
                //SceneLoader.log("async allow scene activation ? " + async.allowSceneActivation);

                yield return new WaitUntil(() => sc.isLoaded);

                if (SceneLoader.verbose) SceneLoader.log(sc.name + " :   is now loaded");
            }

            assoc.handle = sc;

            if (SceneLoader.verbose) SceneLoader.log("  L <b>" + target + "</b> is loaded");

            // feeders of this scene
            Coroutine feeders = solveFeeders(sc, () =>
            {
                feeders = null;
            });
            while (feeders != null) yield return null;

            if (SceneLoader.verbose) SceneLoader.log("'<b>" + target + "</b>' setup");
        }


        public Coroutine solveFeeders(Scene scene, Action onComplete = null)
        {
            return StartCoroutine(processFeeders(scene, onComplete));
        }

        IEnumerator processFeeders(Scene scene, Action onFeedersCompleted = null)
        {
            ///// feeder, additionnal scenes (from feeder script)
            GameObject[] roots = scene.GetRootGameObjects();
            List<fwp.scenes.feeder.SceneLoaderFeederBase> feeders = new();
            for (int i = 0; i < roots.Length; i++)
            {
                feeders.AddRange(roots[i].GetComponentsInChildren<fwp.scenes.feeder.SceneLoaderFeederBase>());
            }

            if (feeders.Count > 0)
            {
                //start
                for (int i = 0; i < feeders.Count; i++)
                {
                    feeders[i].doFeed();
                }

                if (SceneLoader.verbose) SceneLoader.log("waiting for x" + feeders.Count + " feeders");

                // wait for all feeders to be done
                for (int i = 0; i < feeders.Count; i++)
                {
                    yield return new WaitUntil(() => feeders[i] == null);
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
            List<AsyncOperation> asyncsToUnload = new();

            for (int i = 0; i < sceneNames.Length; i++)
            {
                if (sceneNames[i] == null) continue;
                if (sceneNames[i].Length <= 0) continue;

                Scene sc = SceneManager.GetSceneByName(sceneNames[i]);

                if (!sc.IsValid())
                {
                    SceneLoader.logw("scene not valid : " + sceneNames[i]);
                    continue;
                }

                if (!sc.isLoaded)
                {
                    SceneLoader.logw("scene not loaded : " + sceneNames[i]);
                    continue;
                }

                AsyncOperation async = SceneManager.UnloadSceneAsync(sceneNames[i]);
                if (async == null)
                {
                    SceneLoader.logw("no asyncs returned for scene of name " + sceneNames[i]);
                    continue;
                }

                SceneLoader.log("+unload: " + sceneNames[i]);

                asyncsToUnload.Add(async);
            }

            if (SceneLoader.verbose) SceneLoader.log("unload | asked scenes x" + sceneNames.Length + " -> asyncs x" + asyncsToUnload.Count);


            if (asyncsToUnload.Count > 0)
            {
                SceneLoader.log("waiting for asyncs x" + asyncsToUnload.Count);

                //wait for all
                while (asyncsToUnload.Count > 0)
                {
                    yield return new WaitUntil(() => asyncsToUnload[0].isDone);

                    asyncsToUnload.RemoveAt(0);

                    SceneLoader.log("... asyncs x" + asyncsToUnload.Count);
                    yield return null;
                }
            }

            if (onCompletionDelay > 0f)
            {
                SceneLoader.log("unload-post delayed:" + asyncsToUnload.Count);
                yield return new WaitForSeconds(onCompletionDelay);
            }

            onComplete?.Invoke();
            GameObject.Destroy(gameObject);
        }

        /// <summary>
        /// meant to track if framerate is stable before continuing
        /// </summary>
        static public IEnumerator WaitUntilAboveFps(float targetFps = 30f, int consecutiveFrames = 5, float timeout = 5f)
        {
            float maxDelta = 1f / targetFps;
            int okCount = 0;
            float elapsed = 0f;

            while (okCount < consecutiveFrames && elapsed < timeout)
            {
                yield return null;
                float delta = Time.unscaledDeltaTime;

                if (delta < maxDelta)
                    okCount++;
                else
                    okCount = 0;

                elapsed += delta;
            }
        }

    }
}
