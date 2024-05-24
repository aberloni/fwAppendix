using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace fwp.screens
{
    /// <summary>
    /// wrapper to manage a menu using scenes
    /// 1x menu is 1x scene and will be loaded when called
    /// and can be unloaded when the user is done
    /// 
    /// show,hide
    /// updateVisible,updateNotVisible
    /// 
    /// visibility is based on activation/deactivation of first child of this component
    /// to use canvases see ScreenUi
    /// </summary>
    public class ScreenObject : MonoBehaviour
    {
        static public bool verbose = false;

        bool _debug = false;

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
            pauseIngameUpdate = 1, // screen that pauses gameplay
            blockIngameInput = 2,  // screen that lock inputs
            stickyVisibility = 4,  // can't be hidden
            stickyPersistance = 8, // can't be unloaded
            hideOtherLayerOnShow = 16,
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

        public Scene getScene() => gameObject.scene;

        public bool isSticky() => tags.HasFlag(ScreenTags.stickyVisibility);

        /// <summary>
        /// @awake active scene is check
        /// </summary>
        virtual protected bool isDebugContext() => _debug;


        private void Awake()
        {
            _debug = UnityEngine.SceneManagement.SceneManager.GetActiveScene() == gameObject.scene;

            if (type == ScreenType.none)
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

            logScreen("created");
        }

        /// <summary>
        /// stay true until hypotetic engine is ready
        /// </summary>
        virtual protected bool delayEngineCheck()
        {
            return false;
        }

        private IEnumerator Start()
        {
            if (delayEngineCheck())
            {
                while (delayEngineCheck()) yield return null;
                logScreen("delay engine : done");
            }


            // setup will trigger auto opening and setupBeforeOpening
            screenSetup();

            yield return null;

            if (!isDebugContext()) logScreen("-debug => active scene : " + SceneManager.GetActiveScene().name + " != " + gameObject.scene.name);
            else
            {
                logScreen("+debug => screen scene : " + gameObject.scene.name + " is active scene");
                screenSetupDebug();
            }

            // for screen watcher order
            yield return null;
            yield return null;
            yield return null;

            screenSetupLate();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            validate();
        }
#endif

        virtual protected void validate()
        { }

        virtual protected void screenSetup()
        {
            logScreen("setup");
        }

        /// <summary>
        /// before setup late
        /// </summary>
        virtual protected void screenSetupDebug()
        {
            logScreen("setup debug");
        }

        virtual protected void screenSetupLate()
        {
            logScreen("setup late");
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

        /// <summary>
        /// return : visibility
        /// </summary>
        virtual protected bool toggleVisible(bool flag)
        {
            if (isVisible() != flag)
            {
                //log("toggle visible : " + flag);

                if (canvas.hasCanvas())
                {
                    return canvas.toggleVisible(flag);
                }

                // nothing specific ?
                // no failure ...
            }

            return isVisible();
        }

        virtual public bool isVisible()
        {
            if (canvas != null) return canvas.isVisible();
            if (transform.childCount > 0)
            {
                return transform.GetChild(0).gameObject.activeSelf;
            }
            return gameObject.activeSelf;
        }

        [ContextMenu("show instant")]
        protected void ctxm_show()
        {
            if (!show())
            {
                Debug.LogWarning(getStamp() + " couldn't show ?", this);
            }
        }

        [ContextMenu("hide")]
        protected void ctxm_hide()
        {
            if (!forceHide())
            {
                Debug.LogWarning(getStamp() + " couldn't hide ?", this);
            }
        }

        /// <summary>
        /// when already loaded but asking to be shown
        /// </summary>
        public bool show()
        {
            //Debug.Log(getStamp() + " show " + name);
            nav?.resetTimerNoInteraction();

            transform.position = Vector3.zero;

            return toggleVisible(true); // specific case : show instant
        }

        /// <summary>
        /// this is virtual, another screen might do something different
        /// </summary>
        public bool hide()
        {
            //Debug.Log("  <color=white>hide()</color> <b>" + name + "</b>");

            if (tags.HasFlag(ScreenTags.stickyVisibility))
            {
                if (verbose)
                    logwScreen("      can't hide because is setup as sticky");

                return false;
            }

            return toggleVisible(false);
        }

        /// <summary>
        /// returns true if actually toggled
        /// ignore sticky states
        /// </summary>
        public bool forceHide() => toggleVisible(false); // specific case : force hide

        /// <summary>
        /// true = success
        /// </summary>
        public bool unload(bool force = false)
        {
            if (!force && tags.HasFlag(ScreenTags.stickyPersistance))
            {
                Debug.LogWarning("can't unload sticky scenes : " + gameObject.scene.name);
                return false;
            }

            logScreen("unloading <b>" + gameObject.scene.name + "</b>");

            //SceneManager.UnloadSceneAsync(gameObject.scene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
            SceneManager.UnloadSceneAsync(gameObject.scene.name);

            return true;
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
            logScreen("calling <b>home screen</b>");

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
            if (leader != null)
                visi = leader == this;

            toggleVisible(visi); // standby logic
        }

        virtual protected void onScreenDestruction()
        {
            logScreen("destroy");
        }

        virtual public string stringify()
        {
            return "\n  isVisible ? " + isVisible();
        }

        virtual public bool isVerbose() => verbose;

        protected void logwScreen(string ct)
        {
            if (!isVerbose())
                return;

            Debug.LogWarning(getStamp() + " !>> " + ct, this);
        }

        void logScreen(string ct, Component target = null)
        {
            if (!isVerbose())
                return;

            if (target == null) target = this;
            Debug.Log(getStamp() + " >>         " + ct, this);
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