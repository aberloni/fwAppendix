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
	/// 
	/// this layer is not meant to include open/close paradigm
	/// it's meant to make the screen visible or not
	/// use ScreenAnimated for open/close paradigm
	/// </summary>
	public class ScreenObject : MonoBehaviour
	{
		virtual public bool isVerbose => verbose || ScreensManager.isVerbose;
		public bool verbose = false;

		/// <summary>
		/// is context  is debug context
		/// </summary>
		bool _debug = false;

		/// <summary>
		/// boot is done, coroutine ended
		/// </summary>
		public bool IsReady => _ready;
		bool _ready = false;

		const string screenPrefix = "screen-";

		public string getStamp() => Time.frameCount + $"@<color=white>{GetType()}:{name}</color>:   ";

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
			if (canvas == null)
			{
				Debug.LogError("screen framework works only with canvas");
				enabled = false;
				return;
			}

			_debug = SceneManager.GetActiveScene() == gameObject.scene;

			if (type == ScreenType.none)
			{
				Debug.LogWarning("integration:missing screen type", this);
			}

			ScreensManager.subScreen(this);
			screenCreated();
		}

		virtual protected void screenCreated()
		{
			// not shown or hidden :
			// at this abstract level, keep whatever is setup in editor

			logScreen("created");
		}

		/// <summary>
		/// stay true until hypotetic engine is ready
		/// </summary>
		virtual protected IEnumerator delayEngineCheck()
		{
			yield return null;
		}

		private IEnumerator Start()
		{
			Debug.Assert(!_ready, "alreayd ready ?");

			yield return delayEngineCheck();

			// setup will trigger auto opening and setupBeforeOpening
			screenSetup();

			// must be called before late(), late will open the screen (auto open)
			if (isDebugContext())
			{
				yield return null;

				logScreen("+debug:  gameo scene : " + gameObject.scene.name);
				screenSetupDebug();
			}


			// for screen watcher order
			yield return null;
			yield return null;
			yield return null;

			screenSetupLate();

			_ready = true;
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
		/// ask to change visib
		/// this will toggle on/off canvas (if any)
		/// </summary>
		protected void setVisibility(bool flag)
		{
			// no change = do nothing
			if (isVisible() == flag)
			{
				logScreen("visibility? already @ " + flag);
				return;
			}

			if (!flag && tags.HasFlag(ScreenTags.stickyVisibility))
			{
				logwScreen("      can't hide because is setup as sticky");
				return;
			}

			setVisible(flag); // visibility
		}

		/// <summary>
		/// describe how to show/hide screen
		/// this ignores filter checks (sticky)
		/// </summary>
		virtual protected void setVisible(bool flag)
		{
			logScreen("canvas:visibility:" + flag);
			canvas.toggleVisible(flag);
		}

		/// <summary>
		/// routine to describe if screen is visible
		/// </summary>
		virtual public bool isVisible()
		{
			return canvas.isVisible();
		}

		[ContextMenu("force show")]
		protected void ctxm_show()
		{
			setVisible(true); // cm
		}

		[ContextMenu("force hide")]
		protected void ctxm_hide()
		{
			setVisible(false); // cm
		}

		virtual protected bool canOpen()
		{
			return true;
		}

		public void open()
		{
			if (!canOpen())
			{
				logScreen("open: can't");
				return;
			}

			logScreen("open");
			nav?.resetTimerNoInteraction();

			setupBeforeOpening();
			reactOpen();

			// default, canvas needs to be visible
			setVisibility(true);
		}

		/// <summary>
		/// on open()
		/// </summary>
		virtual protected void setupBeforeOpening()
		{ }

		/// <summary>
		/// what to do when opening
		/// (setup is done just before)
		/// </summary>
		virtual public void reactOpen()
		{
			onOpeningEnded();
		}

		virtual protected void onOpeningEnded()
		{ }

		virtual protected bool canClose() => true;

		public void close()
		{
			if (!canClose())
			{
				logScreen("close: can't");
				return;
			}

			logScreen("close");

			setupBeforeClosing();

			reactClose();
		}

		virtual protected void setupBeforeClosing()
		{ }

		/// <summary>
		/// what to do when close is called
		/// </summary>
		virtual public void reactClose()
		{
			onClosingEnded();
		}

		/// <summary>
		/// to call when screen has finished it's closing process/animation
		/// </summary>
		virtual protected void onClosingEnded()
		{
			logScreen("close:onClosingAnimationCompleted");

			if (isUnloadAfterClosing()) //won't if sticky persist
			{
				unload();
			}
			else
			{
				setVisibility(false);
			}

		}

		/// <summary>
		/// allow to change behavior
		/// default : unload the scene after hiding animation is done
		/// </summary>
		virtual public bool isUnloadAfterClosing()
		{
			return !tags.HasFlag(ScreenTags.stickyPersistance);
		}

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

		/// <summary>
		/// when using unity events callbacks
		/// </summary>
		virtual public void act_button(Button clickedButton)
		{
			//process_button_press(clickedButton.name);
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

		virtual protected void onScreenDestruction()
		{
			logScreen("destroy");
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

			setVisibility(visi); // standby logic
		}

#if UNITY_EDITOR
		[ContextMenu("log.stringify")]
		void cmStringify() => Debug.Log(stringify(), this);
#endif

		virtual public string stringify()
		{
			string ret = GetType() + ":" + name;
			if (isVisible()) ret += " VISIBLE";
			if (tags.HasFlag(ScreenTags.stickyVisibility)) ret += " STICKY"; // can't hide
			if (tags.HasFlag(ScreenTags.stickyPersistance)) ret += " PERSIST"; // can't unload
			return ret;
		}

		public bool IsScreen<T>(string nameContains = "") where T : ScreenObject => IsScreen(typeof(T), nameContains);

		public bool IsScreen(Type t, string nameContains = "")
		{
			if (!string.IsNullOrEmpty(nameContains) && !name.Contains(nameContains)) return false;
			if (t != null && GetType() != t) return false;
			return true;
		}

		protected void logwScreen(string msg, object tar = null)
		{
			if (!isVerbose) return;

			if (tar == null) tar = this;

			Debug.LogWarning(getStamp() + msg, tar as UnityEngine.Object);
		}

		protected void logScreen(string msg, object target = null)
		{
			if (!isVerbose) return;

			if (target == null) target = this;

			Debug.Log(getStamp() + msg, target as UnityEngine.Object);
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