using UnityEngine;
using UnityEditor;

namespace fwp.utils.editor
{
	/// <summary>
	/// editor window + tools
	/// </summary>
	public class WinEdWrapper : EditorWindow
	{
		public const float btnSizeXS = 30f;
		public const float btnSizeS = 50f;
		public const float btnSizeM = 75f;
		public const float btnSizeL = 125f;

		protected bool verbose = false;

		virtual protected string getWindowTabName() => GetType().ToString();

		virtual protected bool isDrawableAtRuntime() => true;

		/// <summary>
		/// scroll position within the window
		/// </summary>
		Vector2 winScroll;

		private void OnEnable()
		{
			build();

			// https://forum.unity.com/threads/editorwindow-how-to-tell-when-returned-to-editor-mode-from-play-mode.541578/
			EditorApplication.playModeStateChanged += reactPlayModeState;
			//LogPlayModeState(PlayModeStateChange.EnteredEditMode);
		}

		virtual protected void build()
		{ }

		private void OnDisable()
		{
			EditorApplication.playModeStateChanged -= reactPlayModeState;
		}

		private void OnFocus() => onFocus(true);
		private void OnLostFocus() => onFocus(false);

		virtual protected void onFocus(bool gainFocus)
		{ }

		/// <summary>
		/// when editor changes mode
		/// </summary>
		virtual protected void reactPlayModeState(PlayModeStateChange state)
		{
			//Debug.Log(state);
		}

		readonly GUIContent labelRuntime = new GUIContent("not @ runtime");

		private void OnGUI()
		{
			drawHeader();

			if (!isDrawableAtRuntime() && Application.isPlaying)
			{
				GUILayout.Label(labelRuntime);
				return;
			}

			winScroll = GUILayout.BeginScrollView(winScroll);
			draw();
			GUILayout.EndScrollView();

			drawFooter();
		}

#if UNITY_6000_0_OR_NEWER
		const string iconVerbose = "💬";
#else
		const string iconVerbose = "@";
#endif
#if UNITY_6000_0_OR_NEWER
		const string iconRefresh = "🔄";
#else
		const string iconRefresh = "↺";
#endif

		virtual protected void drawHeader()
		{
			string winName = getWindowTabName();

			GUILayout.BeginHorizontal();

			if (GUILayout.Button(verbose ? "+" + iconVerbose : iconVerbose, QuickEditorViewStyles.gWinTitleButton))
			{
				verbose = !verbose;
				Debug.LogWarning("toggle verbosity : " + verbose);
			}

			if (GUILayout.Button(iconRefresh, QuickEditorViewStyles.gWinTitleButton))
			{
				onRefreshClicked();
			}

			GUILayout.Label(winName, QuickEditorViewStyles.gWinTitle);

			GUILayout.EndHorizontal();
		}

		virtual protected void onRefreshClicked()
		{
			log("<b>refresh</b> clicked");
		}

		/// <summary>
		/// content to draw in editor window
		/// after window title
		/// scroll-able
		/// </summary>
		virtual protected void draw()
		{ }

		/// <summary>
		/// something drawn at the bottom of the window
		/// after the scroll view
		/// </summary>
		virtual protected void drawFooter()
		{ }

		/// <summary>
		/// helper
		/// generic button drawer
		/// </summary>
		static public bool drawButton(string label)
		{
			bool output = false;

			//EditorGUI.BeginDisabledGroup(select == null);
			if (GUILayout.Button(label, getButtonSquare(30f, 100f)))
			{
				output = true;
			}
			//EditorGUI.EndDisabledGroup();

			return output;
		}

		/// <summary>
		/// helper
		/// draw button that react to presence of an object
		/// </summary>
		static public bool drawButtonReference(string label, GameObject select)
		{
			bool output = false;

			EditorGUI.BeginDisabledGroup(select == null);
			if (GUILayout.Button(label, getButtonSquare()))
			{
				output = true;
			}
			EditorGUI.EndDisabledGroup();

			return output;
		}

		/// <summary>
		/// draw a label with speficic style
		/// </summary>
		static public void drawSectionTitle(string label, float spaceMargin = 20f)
		{
			if (spaceMargin > 0f)
				GUILayout.Space(spaceMargin);

			//GUILayout.BeginHorizontal();

			GUILayout.Label(label, QuickEditorViewStyles.gSectionTitle);

			//GUILayout.EndHorizontal();
		}

		static private GUIStyle gButtonSquare;
		static public GUIStyle getButtonSquare(float height = 50, float width = 80)
		{
			if (gButtonSquare == null)
			{
				gButtonSquare = new GUIStyle(GUI.skin.button);
				gButtonSquare.alignment = TextAnchor.MiddleCenter;
				gButtonSquare.fontSize = 10;
				gButtonSquare.fixedHeight = height;
				gButtonSquare.fixedWidth = width;
				gButtonSquare.normal.textColor = Color.white;

				gButtonSquare.wordWrap = true;
			}
			return gButtonSquare;
		}

		/// <summary>
		/// helper : focus an element in editor
		/// +focus in scene view
		/// </summary>
		static public void focusElement(GameObject tar, bool alignViewToObject)
		{
			Selection.activeGameObject = tar;
			EditorGUIUtility.PingObject(tar);

			if (SceneView.lastActiveSceneView != null && alignViewToObject)
			{
				SceneView.lastActiveSceneView.AlignViewToObject(tar.transform);
			}
		}

		/// <summary>
		/// will refresh a window by given type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		static public void setWindowDirty<T>() where T : WinEdRefreshable
		{
			if (EditorWindow.HasOpenInstances<T>())
			{
				var win = EditorWindow.GetWindow<T>();
				win.primeRefresh();
			}
		}

		protected void log(string content)
		{
			if (verbose) Debug.Log(GetType() + ": " + content);
		}

		protected void logw(string content)
		{
			if (verbose) Debug.LogWarning(GetType() + ": " + content);
		}
	}

}
