using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore;

/// <summary>
/// ces écrans ne doivent pas avoir de lien fort avec le maze
/// ils doivent etre TOUS load/unload dynamiquement en fonction des besoins
/// ex : on a pas de raison de les faire réagir au setup de la map
/// </summary>

namespace fwp.screens
{
    abstract public class ScreenAnimated : ScreenObject
    {
        static public List<ScreenAnimated> openedAnimatedScreens = new List<ScreenAnimated>();

        protected Animator _animator;

        Coroutine _coprocOpening;   // opening
        Coroutine _coprocClosing;   // closing
        bool _interactable = false;       // interactable

        /// <summary>
        /// contains all data that can vary in other contexts
        /// </summary>
        public struct ScreenAnimatedParameters
        {
            public string bool_open;
            public string state_closed; // name of the state when screen is closed
            public string state_opened;

            public bool canOpen(Animator a)
            {
                if (!hasParam(a, bool_open)) return false;
                return true;
            }

            /*
            void logStatePresence(Animator a, string state)
            {
                if (!a.HasState(state, 0)) Debug.LogWarning(a.name + " NOK " + state);
                else Debug.LogWarning(a.name + " OK " + state);
            }
            */

            public void logParamPresence(Animator a, string param)
            {
                if (!hasParam(a, param)) Debug.LogWarning(a.name + " NOK " + param);
                else Debug.LogWarning(a.name + " OK " + param);
            }

            public bool hasParam(Animator a, string param)
            {
                foreach (var p in a.parameters)
                {
                    if (p.name == param)
                        return true;
                }
                return false;
            }
        }

        protected ScreenAnimatedParameters parameters;

        //const string STATE_HIDING = "hiding";
        //const string STATE_OPENING = "opening";

        /// <summary>
        /// constructor / awake
        /// </summary>
        protected override void screenCreated()
        {
            base.screenCreated();

            solveAnimator();

            openedAnimatedScreens.Add(this);

            // animated must be hidden by default
            setVisibility(false);
        }

        void solveAnimator()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
                if (_animator == null)
                {
                    // seek one in immediate children only
                    foreach (Transform child in transform)
                    {
                        _animator = child.GetComponent<Animator>();
                        if (_animator != null) break;
                    }
                }
            }

            if (_animator != null)
            {
                // generate params to interact with animator
                parameters = generateAnimatedParams();
                if (!parameters.canOpen(_animator))
                {
                    logwScreen("ignore animator : " + _animator + " not compat", _animator);
                    _animator = null;
                }
            }

            if (_animator == null)
            {
                logwScreen("no animator for animated screen : " + name, this);
            }
        }

        protected override void onScreenDestruction()
        {
            base.onScreenDestruction();

            openedAnimatedScreens.Remove(this);
        }

        /// <summary>
        /// to override if animator have difference states names
        /// </summary>
        virtual protected ScreenAnimatedParameters generateAnimatedParams()
        {
            var _parameters = new ScreenAnimatedParameters();

            _parameters.bool_open = "open";
            _parameters.state_closed = "closed";
            _parameters.state_opened = "opened";

            return _parameters;
        }

        protected override void screenSetupLate()
        {
            base.screenSetupLate();

            if (isOpenDuringSetup()) // true by default
            {
                logScreen("animated:setup:late:auto open");
                open();
            }
        }

        /// <summary>
        /// to prevent screen beeing visible by default
        /// </summary>
        virtual protected bool isOpenDuringSetup() => true;

        public override void reactOpen()
        {
            base.reactOpen();

            //already animating ?

            if (isOpening())
            {
                if (verbose)
                {
                    logwScreen(" => open animated => coroutine d'opening tourne déjà ?");
                    logwScreen(" trying to re-open the same screen during it's opening ?");
                }

                return;
            }

            if (hasValidAnimator())
            {
                logScreen("animation:  open (animation)");
                _coprocOpening = StartCoroutine(processAnimatingOpening());
            }
            else
            {
                logScreen("animation:  open (no animation)");
                onOpeningAnimationDone();
            }

            ScreenLoading.hideLoadingScreen(); // now animating open screen
        }

#if UNITY_EDITOR
        [ContextMenu("validator")]
        protected void cmValidator()
        {
            logwScreen("VALIDATOR");

            //fetch animator ref
            solveAnimator();

            if (!hasValidAnimator())
            {
                logwScreen("animator is not valid");
            }

            if (canvas.canvas == null) logwScreen("no canvas");
            else logwScreen("canvas : " + canvas.canvas, canvas.canvas);
        }
#endif

        virtual protected bool hasValidAnimator()
        {
            if (_animator == null)
            {
                logwScreen("NOK   animator = null");
                return false;
            }

            if (_animator.runtimeAnimatorController == null)
            {
                logwScreen("NOK   animator.controller = null");
                return false;
            }

            return parameters.canOpen(_animator);
        }

        IEnumerator processAnimatingOpening()
        {
            Debug.Assert(hasValidAnimator(), "nop");

            _animator.SetBool(parameters.bool_open, true);

            logScreen("open:wait state:<b>" + parameters.state_opened + "</b>");

            while (checkOpening()) yield return null;

            onOpeningAnimationDone();
        }

        /// <summary>
        /// check if screen is still animating opening
        /// + additionnal checks possible
        /// 
        /// return true keep the process pending
        /// </summary>
        virtual protected bool checkOpening()
        {
            // not reached OPEN-ED state ?
            AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
            if (!info.IsName(parameters.state_opened)) return true;
            return false;
        }

        /// <summary>
        /// do something at the end of opening animation
        /// </summary>
        virtual protected void onOpeningAnimationDone()
        {
            _coprocOpening = null;

            // this is done before "open animation"
            //toggleVisible(true); // opening animation done : jic

            _interactable = true;

            logScreen("animated:opening:done");
        }

        protected override void setupBeforeClosing()
        {
            base.setupBeforeClosing();
            _interactable = false;
        }

        public override void reactClose()
        {
            if (isClosing())
            {
                logwScreen(" ... already closing");
                return;
            }

            // hide/unload
            //base.reactClose();

            if (_coprocClosing != null)
            {
                StopCoroutine(_coprocClosing);
                _coprocClosing = null;
            }

            if (hasValidAnimator())
            {
                logScreen("close (animated)");
                _coprocClosing = StartCoroutine(processAnimatingClosing());
            }
            else
            {
                logScreen("close (not animated)");
                onClosingAnimationCompleted();
            }

        }

        IEnumerator processAnimatingClosing()
        {
            _animator.SetBool(parameters.bool_open, false);

            logScreen("animated:wait state:<b>" + parameters.state_closed + "</b>");

            while (checkClosing()) yield return null;

            onClosingAnimationCompleted();
        }

        virtual protected bool checkClosing()
        {
            AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
            if (!info.IsName(parameters.state_closed)) return true;

            return false;
        }

        /// <summary>
        /// do more stuff after closing
        /// </summary>
        override protected void onClosingAnimationCompleted()
        {
            base.onClosingAnimationCompleted();

            _coprocClosing = null;
            _interactable = false;
        }

        /// <summary>
        /// /! 
        /// APRES anim open
        /// AVANT anim close
        /// </summary>
        public bool isOpened() => _interactable;
        public bool isClosed() => !isVisible();

        public bool isOpening() => _coprocOpening != null;
        public bool isClosing() => _coprocClosing != null;

        /// <summary>
        /// something above ?
        /// </summary>
        virtual protected bool isInteractable() => _interactable;

        /// <summary>
        /// wait for state to start
        /// state to be focused by animator
        /// </summary>
        IEnumerator processWaitUntilState(string state, System.Action onCompletion = null)
        {
            //logScreen(" ... wait for state:" + state);

            AnimatorStateInfo info;

            //wait for state to start
            do
            {
                info = _animator.GetCurrentAnimatorStateInfo(0);
                yield return null;
            }
            while (!info.IsName(state));

            //logScreen("state:" + state + " STARTED");

            onCompletion?.Invoke();
        }

        public override string stringify()
        {
            string ret = base.stringify();

            if (isOpening()) ret += " OPENING";
            if (isClosing()) ret += " CLOSING";
            if (isOpened()) ret += " OPENED";
            if (isClosed()) ret += " CLOSED";

            return ret;
        }

        /// <summary>
        /// search from all opened screens
        /// </summary>
        static public ScreenAnimated getScreen(string screenName)
        {
            ScreenAnimated[] scs = fwp.appendix.qh.gcs<ScreenAnimated>();
            for (int i = 0; i < scs.Length; i++)
            {
                if (scs[i].isScreenOfSceneName(screenName)) return scs[i];
            }
            return null;
        }

        static public T getScreen<T>(string screenName) where T : ScreenAnimated
        {
            T[] scs = fwp.appendix.qh.gcs<T>();

            if (scs.Length <= 0) Debug.LogWarning("no screen <" + typeof(T) + "> present (to return screen of name : " + screenName + ")");
            else
            {
                for (int i = 0; i < scs.Length; i++)
                {
                    if (scs[i].isScreenOfSceneName(screenName)) return scs[i];
                }
            }

            return null;
        }

        static public void toggleScreen(string screenName)
        {
            ScreenAnimated so = (ScreenAnimated)ScreensManager.getOpenedScreen(screenName);

            // present ?
            if (so != null)
            {
                if (so.isOpened()) so.close();
                else if (so.isClosed()) so.open();
                else
                {
                    Debug.LogWarning("could not solve toggle state of " + screenName, so);
                }
            }
            else
            {
                // not there, load & open
                ScreensManager.open(screenName);
            }

        }

    }

}
