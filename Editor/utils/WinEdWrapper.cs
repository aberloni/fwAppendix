using UnityEditor;
using UnityEngine;

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

		/// <summary>
		/// scroll position within the window
		/// </summary>
		Vector2 winScroll;

		private void OnFocus()
		{
			onFocus(true);
		}

		private void OnLostFocus()
		{
			onFocus(false);
		}

		virtual protected void onFocus(bool gainFocus)
		{ }

		private void OnEnable()
		{
			titleContent = new GUIContent(getWindowTabName());

			// https://forum.unity.com/threads/editorwindow-how-to-tell-when-returned-to-editor-mode-from-play-mode.541578/
			EditorApplication.playModeStateChanged += reactPlayModeState;
			//LogPlayModeState(PlayModeStateChange.EnteredEditMode);
		}

		private void OnDisable()
		{
			EditorApplication.playModeStateChanged -= reactPlayModeState;
		}

		/// <summary>
		/// when editor changes mode
		/// </summary>
		virtual protected void reactPlayModeState(PlayModeStateChange state)
		{
			//Debug.Log(state);
		}

		private void Update()
		{
			update();

			if (!Application.isPlaying)
				updateEditime();
			else
				updateRuntime();
		}

		virtual protected void update()
		{ }

		virtual protected void updateEditime()
		{ }

		virtual protected void updateRuntime()
		{ }

		private void OnGUI()
		{
			drawHeader();

			if (!isDrawableAtRuntime())
			{
				GUILayout.Label("not @ runtime");
				return;
			}

			winScroll = GUILayout.BeginScrollView(winScroll);
			draw();
			GUILayout.EndScrollView();

			drawFooter();
		}
		virtual protected bool isDrawableAtRuntime() => true;

		virtual protected void drawHeader()
		{
			string winName = getWindowTabName();

			GUILayout.BeginHorizontal();

			string vLabel = verbose ? "!@" : "@";

#if UNITY_6000_0_OR_NEWER
            vLabel = verbose ? "!🐛" : "🐛";
#endif

			if (GUILayout.Button(vLabel, QuickEditorViewStyles.WinTitleButton))
			{
				verbose = !verbose;
				Debug.LogWarning("toggle verbosity : " + verbose);
			}

			string rLabel = "↺";

#if UNITY_6000_0_OR_NEWER
            rLabel = "🔄";
#endif

			if (GUILayout.Button(rLabel, QuickEditorViewStyles.WinTitleButton))
			{
				onTitleClicked();
			}

			GUILayout.Label(winName, QuickEditorViewStyles.WinTitle);

			GUILayout.EndHorizontal();
		}

		virtual protected void onTitleClicked()
		{
			log("<b>title clicked</b>");
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
		static public void drawSectionTitle(string label, float spaceMargin = 20f, int leftMargin = 10)
		{
			if (spaceMargin > 0f)
				GUILayout.Space(spaceMargin);

			//GUILayout.BeginHorizontal();

			GUILayout.Label(label, QuickEditorViewStyles.getSectionTitle(15, TextAnchor.UpperLeft, leftMargin));

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
			if (verbose) Debug.Log(GetType() + " @ " + content);
		}
	}

}
