using UnityEditor;
using UnityEngine;

public class WinEdWrapper : EditorWindow
{

    protected bool verbose = false;

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
    }

    virtual protected string getWindowTabName() => GetType().ToString();

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

    protected void log(string content)
    {
        if (verbose) Debug.Log(GetType() + " @ " + content);
    }
}
