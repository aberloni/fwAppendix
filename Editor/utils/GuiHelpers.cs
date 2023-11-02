using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace fwp.appendix
{
    static public class GuiHelpers
    {

        public const float btnSymbWidth = 40f;

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

        static public void pingScene(string path)
        {

            if (!path.StartsWith(pathAssetFolderPrefix)) path = System.IO.Path.Combine(pathAssetFolderPrefix, path);
            if (!path.EndsWith(pathAssetExtension)) path = path + pathSceneExtension;

            Debug.Log("pinging (scene) @ " + path);

            var guid = AssetDatabase.GUIDFromAssetPath(path);
            //Debug.Log(guid);

            var uObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            Selection.activeObject = uObj;

            var id = uObj.GetInstanceID();
            EditorGUIUtility.PingObject(id);
        }
    }

}
