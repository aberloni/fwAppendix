using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

abstract public class EdWinTabs : EdWinRefreshable
{
    public const float btnSize = 40f;

    public struct WinTabsState
    {
        public List<(string, System.Action)> tabs;
        public GUIContent[] tabsContent;
    }

    public WinTabsState editime;
    public WinTabsState runtime;

    /// <summary>
    /// tab label, gui draw callback
    /// </summary>
    abstract public (string, System.Action)[] getTabs();
    virtual public (string, System.Action)[] getTabsRuntime() => getTabs();

    public int getSelectedTab()
    {
        return EditorPrefs.GetInt(GetType().ToString(), 0);
    }

    public void selectDefaultTab()
    {
        EditorPrefs.SetInt(GetType().ToString(), 0);
    }

    public void selectTab(int index)
    {
        EditorPrefs.SetInt(GetType().ToString(), index);
    }

    protected override void refresh(bool force = false)
    {
        if(force || editime.tabsContent.Length <= 0)
        {
            var data = getTabs();
            editime = generateState(data);

            data = getTabsRuntime();
            runtime = generateState(data);

            selectDefaultTab();
        }
    }

    WinTabsState generateState((string, System.Action)[] data)
    {
        WinTabsState state = new WinTabsState();
        state.tabs = new List<(string, System.Action)>();

        foreach (var tab in getTabs())
        {
            state.tabs.Add((tab.Item1, tab.Item2));
        }

        List<string> labels = new List<string>();
        foreach (var kp in state.tabs)
        {
            labels.Add(kp.Item1);
        }
        state.tabsContent = TabsHelper.generateTabsDatas(labels.ToArray());

        return state;
    }

    sealed protected override void draw()
    {
        base.draw();

        int tabIndex = getSelectedTab();

        //Debug.Log(tabIndex);
        var _state = Application.isPlaying ? runtime : editime;
        var _tabIndex = drawTabsHeader(tabIndex, _state.tabsContent);

        if(_tabIndex != tabIndex)
        {
            selectTab(_tabIndex);
        }

        tabIndex = _tabIndex;

        _state.tabs[tabIndex].Item2?.Invoke();
    }

    static public int drawTabsHeader(int tabSelected, GUIContent[] tabs)
    {
        //GUIStyle gs = new GUIStyle(GUI.skin.button)
        //int newTab = GUILayout.Toolbar((int)tabSelected, modeLabels, "LargeButton", GUILayout.Width(toolbarWidth), GUILayout.ExpandWidth(true));
        int newTab = GUILayout.Toolbar((int)tabSelected, tabs, "LargeButton");
        //if (newTab != (int)tabSelected) Debug.Log("changed tab ? " + tabSelected);

        return newTab;
    }

}
