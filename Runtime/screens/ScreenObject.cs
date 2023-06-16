using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace fwp.screens
{
    /// <summary>
    /// show,hide
    /// updateVisible,updateNotVisible
    /// 
    /// visibility is based on activation/deactivation of first child of this component
    /// to use canvases see ScreenUi
    /// </summary>
    public class ScreenObject : MonoBehaviour
    {
        static public bool verbose = false;

        const string screenPrefix = "screen-";

        public string getStamp() => "<color=white>screen</color>:" + name;

        public enum ScreenType
        {
            none = 0,
            menu, // nothing running in background
            overlay, // ingame overlays
        }

        /// <summary>
        /// cumulative states for screens
        /// </summary>
        [System.Flags]
        public enum ScreenTags
        {
            none = 0,
            pauseIngameUpdate       = 1, // screen that pauses gameplay
            blockIngameInput        = 2,  // screen that lock inputs
            stickyVisibility        = 4,  // can't be hidden
            stickyPersistance       = 8, // can't be unloaded
            hideOtherLayerOnShow    = 16,
        };

        public ScreenType type;
        public ScreenTags tags;

        ScreenNav nav;

        ScreenModCanvas _canvas;
        public ScreenModCanvas canvas
        {
            get
            {
                if (_canvas == null) _canvas = new ScreenModCanvas(this);
                return _canvas;
            }
        }

        void Awake()
        {
            if(type == ScreenType.none)
            {
                Debug.LogWarning("integration:missing screen type", this);
            }

            ScreensManager.subScreen(this);
            screenCreated();
        }

        virtual protected void screenCreated()
        {
            // at this abstract level, keep whatever is setup in editor
            //hide(); // default state is : not visible
        }

        /// <summary>
        /// stay true until hypotetic engine is ready
        /// </summary>
        virtual protected bool delayEngineCheck()
        {
            return false;
        }

        IEnumerator Start()
        {
            while (delayEngineCheck()) yield return null;

            // setup will trigger auto opening and setupBeforeOpening
            screenSetup();

            yield return null;

            if (isActiveScene())
            {
                screenSetupDebug();
            }

            // for screen watcher order
            yield return null;
            yield return null;
            yield return null;

            screenSetupLate();
        }

        public Scene getScene() => gameObject.scene;

        public bool isSticky() => tags.HasFlag(ScreenTags.stickyVisibility);

        protected bool isActiveScene()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene() == gameObject.scene;
        }

        virtual protected void screenSetup()
        {

        }

        virtual protected void screenSetupDebug()
        {
            Debug.LogWarning("screen debug setup", this);
        }

        virtual protected void screenSetupLate()
        {

        }

        public void subNavDirection(Action down, Action up, Action left, Action right)
        {
            if (nav == null) nav = new ScreenNav();

            if (down != null) nav.onPressedDown += down;
            if (up != null) nav.onPressedUp += up;
            if (left != null) nav.onPressedLeft += left;
            if (right != null) nav.onPressedRight += right;
        }

        public void subSkip(Action skip)
        {
            if (nav == null) nav = new ScreenNav();
            nav.onBack += skip;
        }

        virtual public void reset()
        { }

        private void Update()
        {
            menuUpdate();
        }

        /// <summary>
        /// must be udpated externaly
        /// update entry point
        /// </summary>
        virtual public void menuUpdate()
        {
            if (isVisible()) updateScreenVisible();
            else updateScreenNotVisible();
        }

        virtual protected void updateScreenNotVisible() { }
        virtual protected void updateScreenVisible()
        {
            nav?.update();
        }

        virtual protected void action_back() { }

        virtual protected bool toggleVisible(bool flag)
        {
            Debug.Log(getStamp() + " toggle visible : " + flag);
            
            if (canvas.hasCanvas())
            {
                return canvas.toggleVisible(flag);
            }

            //fallback
            // this is not good : it's best to no deactivate the screenobject mono
            /*
            if(transform.childCount > 0)
            {
                transform.GetChild(0).gameObject.SetActive(flag);
            }
            else
            {
                gameObject.SetActive(flag);
            }
            */

            return flag;
        }

        virtual public bool isVisible()
        {
            if (canvas != null) return canvas.isVisible();
            if(transform.childCount > 0)
            {
                return transform.GetChild(0).gameObject.activeSelf;
            }
            return gameObject.activeSelf;
        }


        [ContextMenu("show instant")]
        protected void ctxm_show() { showInstant(); }

        [ContextMenu("hide")]
        protected void ctxm_hide() { forceHide(); }

        public void show() => showInstant();
        public void hide() => hideInstant();

        /// <summary>
        /// when already loaded but asking to be shown
        /// </summary>
        public void showInstant()
        {
            //Debug.Log(getStamp() + " show " + name);
            nav?.resetTimerNoInteraction();

            transform.position = Vector3.zero;

            toggleVisible(true); // specific case : show instant

            //Debug.Log(name + " -> show");
        }

        /// <summary>
        /// this is virtual, another screen might do something different
        /// </summary>
        public void hideInstant()
        {
            //Debug.Log("  <color=white>hide()</color> <b>" + name + "</b>");

            if (tags.HasFlag(ScreenTags.stickyVisibility))
            {
                //Debug.LogWarning("    can't hide " + name + " because is setup as sticky");
                return;
            }

            forceHide();
        }

        /// <summary>
        /// returns true if actually toggled
        /// </summary>
        /// <returns></returns>
        public bool forceHide()
        {
            if (isVisible())
            {
                //dans le cas où y a pas que des canvas
                //ou qu'il y a une seule camera ppale et qu'il faut aligner les choses à 0f
                transform.position = Vector3.down * 3000f;

                toggleVisible(false); // specific case : force hide

                return true;
            }

            return false;
        }

        public void unload() => unload(false);
        public void unload(bool force = false)
        {
            if (!force && tags.HasFlag(ScreenTags.stickyPersistance))
            {
                Debug.LogWarning("can't unload sticky scenes : " + gameObject.scene.name);
                return;
            }

            Debug.Log(getStamp()+ " unloading <b>" + gameObject.scene.name + "</b>");

            SceneManager.UnloadSceneAsync(gameObject.scene.name);
        }

        public bool isInteractive() => nav != null;

        public void act_button(Button clickedButton)
        {
            process_button_press(clickedButton.name);
        }

        virtual protected void process_button_press(string buttonName)
        { }

        virtual public void act_call_home()
        {
            Debug.Log(getStamp() + " calling <b>home screen</b>");

            ScreensManager.open(ScreensManager.ScreenNameGenerics.home);
        }

        /// <summary>
        /// screen_[name]
        /// </summary>
        public string extractName()
        {
            string[] split = name.Split('_'); // (screen_xxx)
            return split[1].Substring(0, split[1].Length - 1); // remove ')'
        }

        public bool isScreenOfSceneName(string nm)
        {
            //Debug.Log(nm + " vs " + gameObject.scene.name);
            return gameObject.scene.name.EndsWith(nm);
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying) return;

            onScreenDestruction();
            ScreensManager.unsubScreen(this);
        }

        /// <summary>
        /// to toggle all screens that are not leader
        /// </summary>
        public void setStandby(ScreenObject leader)
        {
            // no leader = visible
            bool visi = leader == null;
            
            // is this screen leader
            if(leader != null)
                visi = leader == this;

            toggleVisible(visi); // standby logic
        }

        virtual protected void onScreenDestruction()
        { }

        public string stringify()
        {
            return "\n  isVisible ? " + isVisible();
        }

        // SHKS

        static public ScreenWatcher callScreen(ScreenOverlay screen, Action onCompletion)
            => callScreen(screen, null, null, onCompletion);

        static public ScreenWatcher callScreen(ScreenOverlay screen,
            Action onCreated = null, Action onOpened = null, Action onClosed = null)
        {
            return ScreenWatcher.create(screenPrefix + screen.ToString(), onCreated, onOpened, onClosed);
        }
    }
}