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
            ScreenWatcher tsw = new GameObject("{temp-" + UnityEngine.Random.Range(0, 10000) + "}").AddComponent<ScreenWatcher>();
            tsw.launch(targetScreen, onOpened, onCompletion);
            return tsw;
        }

        protected string tarScreen;
        protected Action onOpened;
        protected Action onCompletion;

        public ScreenAnimated screen;

        public ScreenWatcher launch(string targetScreen, Action onOpened = null, Action onCompletion = null)
        {
            tarScreen = targetScreen;

            this.onOpened = onOpened;
            this.onCompletion = onCompletion;

            StartCoroutine(globalProcess());

            return this;
        }


        IEnumerator globalProcess()
        {
            yield return null;
            yield return null;
            yield return null;

            Coroutine co = null;

            Debug.Log(" ... waiting for creation ...");

            co = StartCoroutine(resourceCreate());
            while (co != null) yield return null;

            Debug.Log(" ... waiting for opening ...");

            co = StartCoroutine(resourceOpen());
            while (co != null) yield return null;
            onOpened?.Invoke();

            Debug.Log(" ... waiting for closing ...");

            co = StartCoroutine(resourceClose());
            while (co != null) yield return null;

            Debug.Log(" ... waiting for removal ...");

            co = StartCoroutine(resourceDestroy());
            while (co != null) yield return null;

            onCompletion?.Invoke();

            //remove watcher
            GameObject.Destroy(gameObject);
        }



        IEnumerator resourceCreate()
        {
            bool loading = true;

            ScreensManager.open(tarScreen, delegate (ScreenObject screen)
            {
                loading = false;
                this.screen = (ScreenAnimated)screen;
                Debug.Assert(this.screen != null, "null screen ? not animated screen ?");

                //Debug.Log($"{resourceName} screen opened");

                Debug.Assert(screen != null);
            });

            Debug.Log(" ... waiting for screen to be loaded ...");
            while (loading) yield return null;

            onCompletion?.Invoke();
        }

        IEnumerator resourceOpen()
        {
            while (screen == null) yield return null;

            //at least one canvas visible
            while (!screen.isVisible()) yield return null;

            onCompletion?.Invoke();
        }

        IEnumerator resourceClose()
        {
            Debug.Log(" ... wait for closing ...");
            while (screen.isClosing()) yield return null;

            Debug.Log(" ... wait while still flagged as opened ...");
            while (screen.isOpen()) yield return null;

            onCompletion?.Invoke();
        }

        IEnumerator resourceDestroy()
        {
            while (screen != null) yield return null;

            onCompletion?.Invoke();
        }

    }

}
