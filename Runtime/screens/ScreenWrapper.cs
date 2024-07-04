using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace fwp.screens
{

    /// <summary>
    /// loading
    /// open
    /// do something
    /// close
    /// unload
    /// </summary>
    public class ScreenWrapper : MonoBehaviour
    {
        static public ScreenWrapper call(System.Enum enu, Action onLoaded = null, Action onEnded = null)
        {
            return call(enu.ToString(), onLoaded, onEnded);
        }

        static public ScreenWrapper call(string screenName, Action onLoaded = null, Action onEnded = null)
        {
            GameObject obj = new GameObject("~sw-" + screenName);
            return obj.AddComponent<ScreenWrapper>().setup(screenName, onLoaded, onEnded);
        }

        ScreenObject screen;

        Action onLoaded;
        Action onEnded;

        public ScreenWrapper setup(string nm, Action onLoaded = null, Action onEnded = null)
        {
            this.onLoaded = onLoaded;
            this.onEnded = onEnded;

            //Debug.Log(name + " setup");

            StartCoroutine(processWrapper(nm));

            return this;
        }

        IEnumerator processWrapper(string nm)
        {
            Debug.Log("wrapper process : loading screen " + nm);

            ScreensManager.open(nm, (ScreenObject loadedScreen) =>
            {
                screen = loadedScreen;
            });

            Debug.Log("wrapper process : waiting for screen ("+nm+")");

            // wait for the screen
            while (screen == null) yield return null;

            this.onLoaded?.Invoke();

            Debug.Log("wrapper process : wait for screen to close");

            // wait for screen to close
            while (this.screen != null) yield return null;

            Debug.Log("wrapper process : screen is null");

            this.onEnded?.Invoke();

            GameObject.Destroy(gameObject);
        }

        public bool isScreenExisting()
        {
            return screen != null;
        }

        public bool isScreenOpened()
        {
            return screen.isVisible();
        }
    }

}
