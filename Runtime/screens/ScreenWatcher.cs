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
        public bool verbose = false;

        static public ScreenWatcher create(string closeScreen, string targetScreen, Action onCreated = null)
        {
            ScreenWatcher tsw = generate(targetScreen);
            tsw.closeAndLaunch(closeScreen, targetScreen, onCreated);
            return tsw;
        }

        static public ScreenWatcher create(string targetScreen, Action onCreated = null, Action onOpened = null, Action onCompletion = null)
        {
            ScreenWatcher tsw = generate(targetScreen);
            tsw.launch(targetScreen, onCreated, onOpened, onCompletion);
            return tsw;
        }

        static ScreenWatcher generate(string targetScreen)
        {
            ScreenWatcher tsw = getExisting(targetScreen);

            if (tsw == null)
            {
                tsw = new GameObject("{watcher-" + UnityEngine.Random.Range(0, 10000) + "}").AddComponent<ScreenWatcher>();
                //tsw.launch(targetScreen, onCreated, onOpened, onCompletion);
            }
            else
            {
                Debug.LogWarning($"another watcher exists for screen <b>{targetScreen}</b>", tsw);
            }

            Debug.Assert(tsw != null, "no watcher for " + targetScreen);

            return tsw;
        }

        static protected ScreenWatcher getExisting(string targetScreen)
        {
            ScreenWatcher[] watchers = fwp.appendix.qh.gcs<ScreenWatcher>();
            for (int i = 0; i < watchers.Length; i++)
            {
                if (watchers[i].isWatching(targetScreen)) return watchers[i];
            }
            return null;
        }

        protected string tarScreen;

        protected Action onScreenCreated;
        protected Action onScreenOpened;

        protected Action onWatchCompletion;

        public ScreenAnimated screen;

        public ScreenWatcher closeAndLaunch(string closeScreen, string targetScreen, Action onCreated = null)
        {
            tarScreen = targetScreen;

            this.onScreenCreated = onCreated;
            this.onScreenOpened = null;
            this.onWatchCompletion = null;

            StartCoroutine(closeProcess(closeScreen, () =>
            {
                StartCoroutine(globalProcess());
            }));

            return this;
        }

        public ScreenWatcher launch(string targetScreen, 
            Action onCreated = null,
            Action onOpened = null, 
            Action onCompletion = null)
        {
            tarScreen = targetScreen;

            this.onScreenCreated = onCreated;
            this.onScreenOpened = onOpened;
            this.onWatchCompletion = onCompletion;
            
            StartCoroutine(globalProcess());
            
            return this;
        }

        public bool isWatching(string targetScreen)
        {
            return tarScreen == targetScreen;
        }

        /// <summary>
        /// close active screen
        /// </summary>
        public void interrupt()
        {
            screen.close();
        }

        IEnumerator closeProcess(string toClose, Action onCompletion)
        {
            var screen = ScreensManager.getOpenedScreen(toClose);
            var screenAnim = screen as ScreenAnimated;

            if (screenAnim.isUnloadAfterClosing())
            {
                Debug.LogError(screenAnim.name + "won't unload ! remove stick persist tag to use watcher", screenAnim);
                onCompletion?.Invoke();
                yield break;
            }

            // start closing process
            if (screenAnim != null) screenAnim.close();

            // wait for screen to be unloaded
            while (screen != null) yield return null;
            
            onCompletion?.Invoke();
        }

        IEnumerator globalProcess()
        {
            yield return null;
            yield return null;
            yield return null;

            Coroutine co = null;

            if(verbose)
                Debug.Log(" ... waiting for creation ...");

            co = StartCoroutine(resourceCreate(()=> {

                onScreenCreated?.Invoke();

                co = null;
            }));

            while (co != null) yield return null;

            if (verbose)
                Debug.Log(" ... waiting for opening ...");

            co = StartCoroutine(resourceOpen(() => { co = null; }));
            while (co != null) yield return null;
            onScreenOpened?.Invoke();

            if (verbose)
                Debug.Log(" ... waiting for closing ...");

            co = StartCoroutine(resourceClose(() => { co = null; }));
            while (co != null) yield return null;

            if (verbose)
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

            if (verbose)
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
            if (verbose)
                Debug.Log(" ... wait for closing ...");

            while (screen.isClosing()) yield return null;

            if (verbose)
                Debug.Log(" ... wait while still flagged as opened ...");

            while (screen.isOpened()) yield return null;

            onCompletion?.Invoke();
        }

        IEnumerator resourceDestroy(Action onCompletion)
        {
            while (screen != null) yield return null;

            onCompletion?.Invoke();
        }

    }

}
