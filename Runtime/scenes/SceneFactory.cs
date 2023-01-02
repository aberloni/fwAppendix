using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

namespace fwp.scenes
{
    /// <summary>
    /// object to provide a way to load sub-scenes
    /// </summary>
    abstract public class SceneFactory : MonoBehaviour
    {
        static List<SceneFactory> _instances = new List<SceneFactory>();

        protected List<AsyncOperation> _asyncs;

        protected string[] systemList;
        public string[] scenes;

        virtual protected void Awake()
        {
            _instances.Add(this);

            //default is empty array
            if (scenes == null) scenes = new string[0];
        }

        virtual protected void Start()
        {
            call_loading_system();
        }

        void call_loading_system()
        {
            define_scenes_list();

            List<string> allScenes = new List<string>();

            if (systemList.Length > 0) allScenes.AddRange(systemList);
            if (scenes.Length > 0) allScenes.AddRange(scenes);

            _asyncs = new List<AsyncOperation>();

            for (int i = 0; i < allScenes.Count; i++)
            {
                StartCoroutine(process_loadScene(allScenes[i]));
            }

            StartCoroutine(waitForAsyncs(setupAfterLoading));
        }

        void setupAfterLoading()
        {
            Debug.Log("SceneFactory | now removing guides ...");

            //SceneTools.removeGuides();

            //check for children
            MonoBehaviour[] comps = gameObject.GetComponents<MonoBehaviour>();
            if (comps.Length > 1)
            {
                Debug.LogError("factory will be destroy, ISSUE on " + gameObject.name + " ↓↓↓");
                Debug.LogError("   L no other component must be on gameobject, counted x" + comps.Length);
                for (int i = 0; i < comps.Length; i++) Debug.Log(comps[i].GetType());
            }

            _instances.Remove(this);

            GameObject.DestroyImmediate(gameObject);
        }

        protected IEnumerator waitForAsyncs(Action onDone)
        {
            //Debug.Log(_asyncs.Count + " asyncs loading");

            while (_asyncs.Count > 0) yield return null;

            onDone?.Invoke();
        }

        protected IEnumerator process_loadScene(string sceneLoad)
        {

            //can't reload same scene
            if (appendix.AppendixUtils.isSceneOpened(sceneLoad)) yield break;

            AsyncOperation async = SceneManager.LoadSceneAsync(sceneLoad, LoadSceneMode.Additive);
            _asyncs.Add(async);

            //Debug.Log("  package '<b>" + sceneLoad + "</b>' | starting loading");

            while (!async.isDone) yield return null;

            _asyncs.Remove(async);

            //Debug.Log("  package '<b>" + sceneLoad + "</b>' | done loading (" + _asyncs.Count + " left)");
        }

        /// <summary>
        /// define here contextual scene loading
        /// </summary>
        virtual protected void define_scenes_list()
        {
            systemList = new string[] { "resources-engine" };
        }

        static public bool isLoading()
        {
            if (_instances == null) return false;
            return _instances.Count > 0;
        }
    }

}
