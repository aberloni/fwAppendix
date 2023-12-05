using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace fwp.utils.editor
{
    using fwp.appendix;

    /// <summary>
    /// PROVIDE:
    /// refresh, update, draw
    /// 
    /// meant to provide a way to force refresh of a window
    /// and encapsulate the refresh content event
    /// </summary>
    abstract public class EdWinRefreshable : EdWinFilterable
    {
        bool _refresh = false;

        virtual protected bool isDrawableAtRuntime() => true;

        static public void setDirty<T>() where T : EdWinRefreshable
        {
            if(EditorWindow.HasOpenInstances<T>())
            {
                var win = EditorWindow.GetWindow<T>();
                win.primeRefresh();
            }
        }

        private void OnFocus()
        {
            onFocus(true);
        }

        private void OnLostFocus()
        {
            onFocus(false);
        }

        virtual protected void onFocus(bool gainFocus)
        {
            if(gainFocus)
            {
                refresh();
            }
        }

        private void OnEnable()
        {
            // https://forum.unity.com/threads/editorwindow-how-to-tell-when-returned-to-editor-mode-from-play-mode.541578/
            EditorApplication.playModeStateChanged += reactPlayModeState;
            //LogPlayModeState(PlayModeStateChange.EnteredEditMode);
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= reactPlayModeState;
        }

        virtual protected void reactPlayModeState(PlayModeStateChange state)
        {
            //Debug.Log(state);
        }

        private void Update()
        {
            if (_refresh)
            {
                _refresh = false;
                refresh(true);
            }

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

        /// <summary>
        /// ask for a refresh
        /// (ie during GUI phase)
        /// </summary>
        protected void primeRefresh() => _refresh = true;

        virtual protected void refreshByTitle()
        {
            verbose = true;
            
            log("title-click");

            refresh(true);
            verbose = false;
        }

        abstract protected void refresh(bool force = false);

        abstract protected string getWindowTitle();

        private void OnGUI()
        {
            if (GUILayout.Button(getWindowTitle(), QuickEditorViewStyles.getWinTitle()))
            {
                refreshByTitle();
            }

            if (!isDrawableAtRuntime())
            {
                GUILayout.Label("not @ runtime");
                return;
            }

            draw();
        }

        /// <summary>
        /// content to draw in editor window
        /// after window title
        /// </summary>
        virtual protected void draw()
        {

        }

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

            GUILayout.Label(label, QuickEditorViewStyles.getSectionTitle(15, TextAnchor.UpperLeft, leftMargin));
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

        static public void focusElementWithIndex(GameObject tar, bool alignViewToObject)
        {
            Selection.activeGameObject = tar;
            EditorGUIUtility.PingObject(tar);

            if (SceneView.lastActiveSceneView != null && alignViewToObject)
            {
                SceneView.lastActiveSceneView.AlignViewToObject(tar.transform);
            }
        }

        /// <summary>
        /// use : EditorGUIUtility.PingObject
        /// </summary>
        static public void pingFolder(string assetsPath)
        {
            if (!assetsPath.StartsWith(GuiHelpers.pathAssetFolderPrefix))
                assetsPath = System.IO.Path.Combine(GuiHelpers.pathAssetFolderPrefix, assetsPath);

            //string path = "Assets/" + assetsPath;

            // Load object
            // https://docs.unity3d.com/ScriptReference/AssetDatabase.LoadAssetAtPath.html
            // must include Assets/
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(assetsPath, typeof(UnityEngine.Object));
            if (obj == null)
            {
                Debug.LogWarning("ping failed @" + assetsPath);
                return;
            }

            // Select the object in the project folder
            Selection.activeObject = obj;

            // Also flash the folder yellow to highlight it
            EditorGUIUtility.PingObject(obj);
        }

    }

}
