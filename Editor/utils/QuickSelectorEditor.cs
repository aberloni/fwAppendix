using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using fwp.utils.editor;

namespace fwp.appendix.utils
{
    /// <summary>
    /// WINDOW EDITOR
    /// 
    /// Syntaxe pour ajouter un tooltip au bouton
    /// GUILayout.Button(new GUIContent("Crossblocker","CrossBlocker"), getButtonSquare());
    /// </summary>
    abstract public class QuickSelectorEditor : EditorWindow
    {
        private void OnGUI()
        {
            GUILayout.Label("Quick Selector", QuickEditorViewStyles.gWinTitle);

            //GUILayout.BeginHorizontal();
            //GUILayout.EndHorizontal();

            drawEditor();

            if (Application.isPlaying)
            {
                GUILayout.Space(10f);

                drawRuntime();
            }

        }

        /// <summary>
        /// generic button drawer
        /// </summary>
        protected void drawButton(GameObject select, string label)
        {
            EditorGUI.BeginDisabledGroup(select == null);
            if (GUILayout.Button(label, getButtonSquare()))
            {
                if (select != null)
                    focusElementWithIndex(select.gameObject, true);
            }
            EditorGUI.EndDisabledGroup();
        }

        protected void drawCurrentSelection()
        {
            GameObject select = UnityEditor.Selection.activeGameObject;

            EditorGUI.BeginDisabledGroup(select == null);
            if (GUILayout.Button("SELECTION\n" + select.name, getButtonSquare()))
            {
                focusElementWithIndex(select.gameObject, true);
            }
            EditorGUI.EndDisabledGroup();
        }

        virtual protected void drawEditor()
        { }

        virtual protected void drawRuntime()
        { }

        int updateIndex(GameObject[] array, int index)
        {
            if (index + 2 > array.Length)
            {
                index = 0;
            }
            else
            {
                index++;
            }
            return index;
        }

        void focusElementWithIndex(GameObject tar, bool alignViewToObject)
        {
            Selection.activeGameObject = tar;
            EditorGUIUtility.PingObject(tar);

            if (SceneView.lastActiveSceneView != null && alignViewToObject)
            {
                SceneView.lastActiveSceneView.AlignViewToObject(tar.transform);
            }
        }

        static private GUIStyle gLittleButton;
        static public GUIStyle getLittleButton()
        {
            if (gLittleButton == null)
            {
                gLittleButton = new GUIStyle(GUI.skin.button);
                gLittleButton.alignment = TextAnchor.MiddleCenter;
                gLittleButton.fixedHeight = 25;
                gLittleButton.fixedWidth = 25;

            }
            return gLittleButton;
        }

        static private GUIStyle gButtonSquare;
        static public GUIStyle getButtonSquare()
        {
            if (gButtonSquare == null)
            {
                gButtonSquare = new GUIStyle(GUI.skin.button);
                gButtonSquare.alignment = TextAnchor.MiddleCenter;
                gButtonSquare.fontSize = 10;
                gButtonSquare.fixedHeight = 50;
                gButtonSquare.fixedWidth = 80;
                gButtonSquare.normal.textColor = Color.black;
            }
            return gButtonSquare;
        }

        static private GUIStyle gTitleBlackBold;
        static public GUIStyle getTitleBlackBold()
        {
            if (gTitleBlackBold == null)

                gTitleBlackBold = new GUIStyle();
            gTitleBlackBold.richText = true;
            gTitleBlackBold.fontStyle = FontStyle.Bold;
            gTitleBlackBold.padding = new RectOffset(10, 5, 2, 2);
            gTitleBlackBold.normal.textColor = Color.black;

            return gTitleBlackBold;
        }

    }

}
