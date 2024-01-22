using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using UnityEditor.IMGUI.Controls;
using Object = UnityEngine.Object;

namespace fwp.appendix
{
    static public class GuiHelpers
    {

        public const float btnSymbWidthSmall = 30f;
        public const float btnSymbWidth = 40f;
        public const float btnLabelWidth = 100f;

        static private GUIStyle gWinTitle;
        static public GUIStyle getWinTitle()
        {
            if (gWinTitle == null)
            {
                gWinTitle = new GUIStyle();

                gWinTitle.richText = true;
                gWinTitle.alignment = TextAnchor.MiddleCenter;
                gWinTitle.normal.textColor = Color.white;
                gWinTitle.fontSize = 20;
                gWinTitle.fontStyle = FontStyle.Bold;
                gWinTitle.margin = new RectOffset(10, 10, 10, 10);
                //gWinTitle.padding = new RectOffset(30, 30, 30, 30);

            }

            return gWinTitle;
        }

        public const string pathAssetFolderPrefix = "Assets";

        const string pathAssetExtension = ".asset";
        const string pathSceneExtension = ".unity";

        /// <summary>
        /// path must end with last "/" (auto add)
        /// </summary>
        static public void selectFolder(string folderPath, bool unfold = false)
        {
            // https://forum.unity.com/threads/selecting-a-folder-in-the-project-via-button-in-editor-window.355357/
            if (folderPath[folderPath.Length - 1] == '/') folderPath = folderPath.Substring(0, folderPath.Length - 1);
            //if (!folderPath.EndsWith('/')) folderPath += "/";

            selectObject(folderPath);
            
            if (unfold)
            {
                EditorApplication.delayCall += () =>
                {
                    waitUpdate(KeyCode.RightArrow);
                };
            }
            
        }

        static public void pingScene(string path)
        {
            if (!path.EndsWith(pathSceneExtension)) path = path + pathSceneExtension;
            selectObject(path);
        }

        /// <summary>
        /// [path].asset
        /// </summary>
        static public void pingAsset(string path)
        {
            // + .asset
            if (!path.EndsWith(pathAssetExtension)) path = path + pathAssetExtension;
            selectObject(path);
        }

        static public bool selectObject(string path)
        {
            // base path
            if (!path.StartsWith(pathAssetFolderPrefix)) path = System.IO.Path.Combine(pathAssetFolderPrefix, path);

            //var guid = AssetDatabase.GUIDFromAssetPath(path);

            // Load object
            // https://docs.unity3d.com/ScriptReference/AssetDatabase.LoadAssetAtPath.html
            // must include Assets/
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

            if (obj == null)
            {
                Debug.LogWarning("LoadAssetAtPath failed @ " + path);
                return false;
            }


            Debug.Log("pinging (scene) @ " + path);

            // https://forum.unity.com/threads/focus-folder-in-project-window.559687/

            if(Selection.activeObject != obj)
            {
                // Select the object in the project folder
                Selection.activeObject = obj;
                return true;
            }

            return false;
        }

        /// <summary>
        /// https://medium.com/@impinq/get-selected-directory-in-unity-editor-25d17e4c38ea
        /// </summary>
        static public void getSelectedFolder()
        {
            string result = null;
            Object[] objs = Selection.GetFiltered<Object>(SelectionMode.Assets);
            foreach (Object obj in objs)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && System.IO.Directory.Exists(path))
                {
                    result = path; break;
                }
            }
        }

        static void waitUpdate(KeyCode key)
        {
            var pt = System.Type.GetType("UnityEditor.ProjectBrowser, UnityEditor");
            EditorWindow pWin = Resources.FindObjectsOfTypeAll(pt)[0] as EditorWindow;
            pWin.Focus();
            //Debug.Log("focus " + pWin);

            //var e = new Event { modifiers = EventModifiers.Alt, keyCode = KeyCode.RightArrow, type = EventType.KeyDown };
            var e = new Event { keyCode = key, type = EventType.KeyDown };
            pWin.SendEvent(e);
            //Debug.Log(key);

        }

    }

}
