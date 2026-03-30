using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

		/// <summary>
		/// contains all data that can vary in other contexts
		/// </summary>
		[System.Serializable]
		public class Parameters
		{
			public Animator animator;

			public string bool_open = "open";
			public string state_opened = "opened";
			public string state_closed = "closed"; // name of the state when screen is closed

			public bool canOpen(Animator a)
			{
				if (!hasParam(a, bool_open)) return false;
				return true;
			}

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


		/// <summary>
		/// state name contained in animator
		/// to be able to track opening/closing
		/// </summary>
		[SerializeField]
		protected Parameters parameters;

		protected Animator Animator => parameters.animator;

		//const string STATE_HIDING = "hiding";
		//const string STATE_OPENING = "opening";

		// INTERNALS

		/// <summary>
		/// constructor / awake
		/// </summary>
		protected override void screenCreated()
		{
			base.screenCreated();

			fetchAnimator();

			openedAnimatedScreens.Add(this);

			// animated must be hidden by default
			// and animated on opening
			setVisibility(false);
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

		protected override void reactBeforeOpening()
		{
			base.reactBeforeOpening();

			callbacks.beforeOpen?.Invoke(this);
		}

		protected override void onScreenDestruction()
		{
			base.onScreenDestruction();

			openedAnimatedScreens.Remove(this);
		}

		/// <summary>
		/// to override if animator have difference states names
		/// </summary>
		virtual protected Parameters generateAnimatedParams() => parameters;

        protected override IEnumerator execOpening()
        {
			logScreen("animation:  open");

			if (hasValidAnimator())
			{
				Animator.SetBool(parameters.bool_open, true);
				logScreen("open:wait state:<b>" + parameters.state_opened + "</b>");
			}

			logScreen("animated.open.check ...");

			yield return base.execOpening();
		}

		/// <summary>
		/// check if screen is still animating opening
		/// + additionnal checks possible
		/// true : keeps the process pending
		/// </summary>
		virtual protected bool checkOpening()
		{
			if (hasValidAnimator())
			{
				// not reached OPEN-ED state ?
				AnimatorStateInfo info = Animator.GetCurrentAnimatorStateInfo(0);
				if (!info.IsName(parameters.state_opened)) return true;
			}

			return false;
		}

		/// <summary>
		/// end of opening animation
		/// </summary>
		protected override void reactAfterOpening()
		{
			base.reactAfterOpening();

			// this is done before "open animation"
			//toggleVisible(true); // opening animation done : jic

			logScreen("animated:opening:done");

			callbacks.afterOpen?.Invoke(this);
		}

		protected override void reactBeforeClosing()
		{
			base.reactBeforeClosing();
			
			callbacks.beforeClose?.Invoke(this);
		}

		IEnumerator processAnimatingClosing()
		{
			logScreen("animated.closing.animated");

			if (hasValidAnimator())
			{
				Animator.SetBool(parameters.bool_open, false);
				logScreen("animatedwait state:<b>" + parameters.state_closed + "</b>");
			}

			logScreen("animated.closing.check");
			yield return new WaitUntil(() => !checkClosing());
			logScreen("animated.closing.check.done");

			reactAfterClosing();
		}

		/// <summary>
		/// return true to keep check active
		/// false : finished
		/// </summary>
		virtual protected bool checkClosing()
		{
			if (hasValidAnimator())
			{
				AnimatorStateInfo info = Animator.GetCurrentAnimatorStateInfo(0);
				if (!info.IsName(parameters.state_closed)) return true;
			}

			return false;
		}
		
		public override string stringify()
		{
			string ret = base.stringify();

			if (isOpening()) ret += " OPENING check?" + checkOpening() + " state?" + parameters.state_opened;
			if (isClosing()) ret += " CLOSING check?" + checkClosing() + " state?" + parameters.state_closed;
			if (isOpened()) ret += " OPENED ?" + isOpened();
			if (isClosed()) ret += " CLOSED ?" + isClosed();

			return ret;
		}

		/// <summary>
		/// called each frame during check open/close
		/// </summary>
		virtual protected bool hasValidAnimator()
		{
			// must have an animator component attached
			if (Animator == null)
				return false;

			// must have a controller
			if (Animator.runtimeAnimatorController == null)
				return false;

			return parameters.canOpen(Animator);
		}

		void fetchAnimator()
		{
			if (parameters == null)
			{
				parameters = generateAnimatedParams();
			}

			if (Animator == null)
			{
				parameters.animator = GetComponent<Animator>();
				if (parameters.animator == null)
				{
					// seek one in immediate children only
					foreach (Transform child in transform)
					{
						parameters.animator = child.GetComponent<Animator>();
						if (parameters.animator != null) break;
					}
				}
			}

			if (Animator == null)
			{
				logwScreen("no animator for animated screen : " + name, this);
			}
		}

#if UNITY_EDITOR

		/// <summary>
		/// list/log possible issues
		/// </summary>
		virtual protected void checkValidator()
		{
			//fetch animator ref
			fetchAnimator();

			if (!hasValidAnimator())
			{
				logwScreen(" ? animator is not valid");
			}

			if (canvas.canvas == null)
			{
				logwScreen(" ? no canvas");
			}
			else logwScreen("canvas : " + canvas.canvas, canvas.canvas);
		}

		[ContextMenu("validator.check")]
		protected void cmValidatorCheck()
		{
			checkValidator();
		}

#endif

		static public void toggleScreen(string screenName)
		{
			ScreenAnimated so = (ScreenAnimated)ScreensManager.getScreen(screenName);

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
