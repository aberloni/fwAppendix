using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace fwp.screens
{

    /// <summary>
    /// 
    /// a component that will exist until the screen is unloaded
    /// 
    /// loading
    ///     onLoaded
    /// open
    ///     onOpened
    /// 
    /// close
    ///     onEnded
    /// 
    /// unload
    /// </summary>
    public class ScreenWrapper : MonoBehaviour
    {
        /// <summary>
        /// enum
        /// </summary>
        static public ScreenWrapper call(Enum enu,
            Action onLoaded = null, Action onOpened = null, Action onEnded = null)
        {
            return call(enu.ToString(), onLoaded, onOpened, onEnded);
        }

        /// <summary>
        /// string name
        /// </summary>
        static public ScreenWrapper call(string screenName,
            Action onLoaded = null, Action onOpened = null, Action onEnded = null)
        {
            GameObject obj = new GameObject("~sw-" + screenName);
            return obj.AddComponent<ScreenWrapper>().setup(screenName,
                onLoaded, onOpened, onEnded);
        }

        ScreenObject screen;

        Action onLoaded;
        Action onOpened;
        Action onEnded;

        public ScreenWrapper setup(string nm,
            Action onLoaded = null, Action onOpened = null, Action onEnded = null)
        {
            this.onLoaded = onLoaded;
            this.onOpened = onOpened;
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

            Debug.Log("wrapper process : waiting for screen (" + nm + ")");

            // wait for the screen
            while (screen == null) yield return null;
            this.onLoaded?.Invoke();

            var animated = screen as ScreenAnimated;
            if (animated != null)
            {
                Debug.Log("wrapper process : waiting for opening");

                while (!animated.isOpened()) yield return null;
            }

            yield return null;

            onOpened?.Invoke();

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
