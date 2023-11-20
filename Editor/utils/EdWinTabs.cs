using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.utils.editor
{
    using fwp.appendix.user;

    /// <summary>
    /// PROVIDE:
    /// tabs for refreshable window
    /// 
    /// usage:
    /// override generate methods to feed your own content
    /// </summary>
    abstract public class EdWinTabs : EdWinRefreshable
    {
        public const float btnSize = 40f;

        const string _editor__profiler_tab = "tab_scene_profiler";

        protected int tabActive
        {
            get => MgrUserSettings.getEdInt(_editor__profiler_tab, 0);
            set => MgrUserSettings.setEdInt(_editor__profiler_tab, value);
        }

        public class WinTabState
        {
            public string path;

            /// <summary>
            /// only use for display
            /// </summary>
            public string label => path.Substring(path.LastIndexOf("/") + 1);

            public System.Func<bool> drawCallback;
            public Vector2 scroll;
        }

        public class WinTabsState
        {
            public List<WinTabState> tabs;

            public GUIContent[] tabsContent; // util for unity drawing

            //public bool isValid() => tabs != null;

            public List<string> labels
            {
                get
                {
                    List<string> output = new List<string>();
                    foreach (var tab in tabs)
                    {
                        output.Add(tab.label);
                    }
                    return output;
                }

            }
        }

        WinTabsState editime;
        WinTabsState runtime;

        protected WinTabsState tabsState => Application.isPlaying ? runtime : editime;

        /// <summary>
        /// what tabs to draw !runtime
        /// tab label, gui draw callback
        /// returns true to draw additionnal content
        /// </summary>
        abstract public (string, System.Func<bool>)[] generateTabsEditor();

        /// <summary>
        /// what to draw @runtime
        /// default is nothing
        /// returns true to draw additionnal content
        /// </summary>
        virtual public (string, System.Func<bool>)[] generateTabsRuntime() => null;

        public void selectDefaultTab() => tabActive = 0;
        public void selectTab(int index) => tabActive = index;

        protected override void refresh(bool force = false)
        {
            //base.refresh(force);

            if (force || editime.tabsContent.Length <= 0)
            {
                if (verbose) Debug.Log("refresh tabs editor");

                var data = generateTabsEditor();
                editime = generateState(data);

                data = generateTabsRuntime();
                if (data != null)
                {
                    if (verbose) Debug.Log("refresh tabs runtime");

                    runtime = generateState(data);
                }

                selectDefaultTab();
            }
        }

        WinTabsState generateState((string, System.Func<bool>)[] data)
        {
            WinTabsState state = new WinTabsState();

            foreach (var tabTuple in data)
            {
                var tab = new WinTabState();
                tab.path = tabTuple.Item1;

                tab.drawCallback = tabTuple.Item2;

                if (state.tabs == null)
                    state.tabs = new List<WinTabState>();

                state.tabs.Add(tab);
            }

            // store stuff for unity drawing
            state.tabsContent = TabsHelper.generateTabsDatas(state.labels.ToArray());

            return state;
        }

        sealed protected override void draw()
        {
            base.draw();

            int currTabIndex = tabActive;

            //Debug.Log(tabIndex);
            var _state = Application.isPlaying ? runtime : editime;

            bool result = false;

            if (_state == null)
            {
                GUILayout.Label("no tabs available");
                result = true;
            }
            else
            {
                // draw labels buttons
                var _tabIndex = drawTabsHeader(currTabIndex, _state.tabsContent);

                // selection changed ?
                if (_tabIndex != currTabIndex)
                {
                    selectTab(_tabIndex); // force selection
                }

                if (_tabIndex < 0)
                    return;

                if(_tabIndex >= _state.tabs.Count)
                {
                    Debug.LogWarning(_tabIndex + " oob ? " + _state.tabs.Count);
                    return;
                }

                var tab = _state.tabs[_tabIndex];

                drawFilterField();

                if (tab.drawCallback != null)
                {
                    tab.scroll = GUILayout.BeginScrollView(tab.scroll);
                    //Debug.Log(tab.scroll);

                    // draw gui
                    result = tab.drawCallback.Invoke();

                    GUILayout.EndScrollView();
                }

            }

            // if tab returned true : issue, don't draw additionnal
            if (!result)
            {
                drawAdditionnal();
            }
        }

        /// <summary>
        /// drawn after tabs zone
        /// </summary>
        virtual protected void drawAdditionnal()
        { }

        static public int drawTabsHeader(int tabSelected, GUIContent[] tabs)
        {
            //GUIStyle gs = new GUIStyle(GUI.skin.button)
            //int newTab = GUILayout.Toolbar((int)tabSelected, modeLabels, "LargeButton", GUILayout.Width(toolbarWidth), GUILayout.ExpandWidth(true));
            int newTab = GUILayout.Toolbar((int)tabSelected, tabs, "LargeButton");
            //if (newTab != (int)tabSelected) Debug.Log("changed tab ? " + tabSelected);

            return newTab;
        }

    }

}
