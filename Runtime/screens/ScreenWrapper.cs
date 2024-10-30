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
            var ret = obj.AddComponent<ScreenWrapper>();

            ret.setup(screenName);
            ret.setupCallbacks(onLoaded, onOpened, onEnded);

            return ret;
        }

        ScreenObject screen;

        Action onLoaded;
        Action onOpened;
        Action onEnded;

        public ScreenWrapper setup(string nm)
        {
            StartCoroutine(processWrapper(nm));

            return this;
        }

        public ScreenObject get() => screen;

        public T get<T>() where T : ScreenObject
        {
            return screen as T;
        }

        /// <summary>
        /// given callbacks will OVERRIDE previous ones
        /// </summary>
        public ScreenWrapper setupCallbacks(Action onLoaded = null, Action onOpened = null, Action onEnded = null)
        {
            if (onLoaded != null) this.onLoaded = onLoaded;
            if (onOpened != null) this.onOpened = onOpened;
            if (onEnded != null) this.onEnded = onEnded;

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
