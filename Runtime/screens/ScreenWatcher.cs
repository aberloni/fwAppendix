using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace fwp.screens
{
    /// <summary>
    /// meant to track opening/closing of a screen
    /// </summary>
    public class ScreenWatcher : MonoBehaviour
    {

        static public ScreenWatcher create(string targetScreen, Action onOpened = null, Action onCompletion = null)
        {
            ScreenWatcher tsw = getExisting(targetScreen);

            if (tsw == null)
            {
                tsw = new GameObject("{temp-" + UnityEngine.Random.Range(0, 10000) + "}").AddComponent<ScreenWatcher>();
                tsw.launch(targetScreen, onOpened, onCompletion);

                return tsw;
            }

            Debug.LogWarning($"another watcher exists for screen <b>{targetScreen}</b>", tsw);

            return null;
        }

        static protected ScreenWatcher getExisting(string targetScreen)
        {
            ScreenWatcher[] watchers = GameObject.FindObjectsOfType<ScreenWatcher>();
            for (int i = 0; i < watchers.Length; i++)
            {
                if (watchers[i].isWatching(targetScreen)) return watchers[i];
            }
            return null;
        }

        protected string tarScreen;
        protected Action onScreenOpened;
        protected Action onWatchCompletion;

        public ScreenAnimated screen;

        public ScreenWatcher launch(string targetScreen, Action onOpened = null, Action onCompletion = null)
        {
            tarScreen = targetScreen;

            this.onScreenOpened = onOpened;
            this.onWatchCompletion = onCompletion;

            StartCoroutine(globalProcess());

            return this;
        }

        public bool isWatching(string targetScreen)
        {
            return tarScreen == targetScreen;
        }

        IEnumerator globalProcess()
        {
            yield return null;
            yield return null;
            yield return null;

            Coroutine co = null;

            Debug.Log(" ... waiting for creation ...");

            co = StartCoroutine(resourceCreate(()=> { co = null; }));
            while (co != null) yield return null;

            Debug.Log(" ... waiting for opening ...");

            co = StartCoroutine(resourceOpen(() => { co = null; }));
            while (co != null) yield return null;
            onScreenOpened?.Invoke();

            Debug.Log(" ... waiting for closing ...");

            co = StartCoroutine(resourceClose(() => { co = null; }));
            while (co != null) yield return null;

            Debug.Log(" ... waiting for removal ...");

            co = StartCoroutine(resourceDestroy(() => { co = null; }));
            while (co != null) yield return null;
            
            onWatchCompletion?.Invoke();

            //remove watcher
            GameObject.Destroy(gameObject);
        }

        private void OnDestroy()
        {
            //onWatchCompletion?.Invoke();
        }


        IEnumerator resourceCreate(Action onCompletion)
        {
            bool loading = true;

            ScreensManager.open(tarScreen, delegate (ScreenObject screen)
            {
                loading = false;
                this.screen = (ScreenAnimated)screen;
                Debug.Assert(this.screen != null, $"null screen, target:{tarScreen} ? not animated screen ?");

                //Debug.Log($"{resourceName} screen opened");

                Debug.Assert(screen != null);
            });

            Debug.Log(" ... waiting for screen to be loaded ...");
            while (loading) yield return null;

            onCompletion?.Invoke();
        }

        IEnumerator resourceOpen(Action onCompletion)
        {
            while (screen == null) yield return null;

            //at least one canvas visible
            while (!screen.isVisible()) yield return null;

            onCompletion?.Invoke();
        }

        IEnumerator resourceClose(Action onCompletion)
        {
            Debug.Log(" ... wait for closing ...");
            while (screen.isClosing()) yield return null;

            Debug.Log(" ... wait while still flagged as opened ...");
            while (screen.isOpen()) yield return null;

            onCompletion?.Invoke();
        }

        IEnumerator resourceDestroy(Action onCompletion)
        {
            while (screen != null) yield return null;

            onCompletion?.Invoke();
        }

    }

}
