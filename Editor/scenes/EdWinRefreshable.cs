using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

/// <summary>
/// meant to provide a way to force refresh of a window
/// and encapsulate the refresh content event
/// </summary>
abstract public class EdWinRefreshable : EditorWindow
{

    bool _refresh = false;

    private void OnEnable()
    {
        refresh(true);
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
            updateOfftime();
        else
            updateRuntime();
    }

    virtual protected void update()
    { }

    virtual protected void updateOfftime()
    { }

    virtual protected void updateRuntime()
    { }

    protected void primeRefresh() => _refresh = true;

    abstract protected void refresh(bool force = false);

    abstract protected string getWindowTitle();

    private void OnGUI()
    {
        if (GUILayout.Button(getWindowTitle(), QuickEditorViewStyles.getWinTitle()))
        {
            Debug.Log("clicked refresh");
            refresh(true);
        }

        draw();
    }

    virtual protected void draw()
    {

    }


    /// <summary>
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

    static bool drawButtonReference(string label, GameObject select)
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

    static public void drawSectionTitle(string label, float spaceMargin = 20f, int leftMargin = 10)
    {
        if(spaceMargin > 0f)
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

    public const string pathAssetFolderPrefix = "Assets";

    /// <summary>
    /// use : EditorGUIUtility.PingObject
    /// </summary>
    static public void pingFolder(string assetsPath)
    {
        if (!assetsPath.StartsWith(pathAssetFolderPrefix))
            assetsPath = System.IO.Path.Combine(pathAssetFolderPrefix, assetsPath);

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
