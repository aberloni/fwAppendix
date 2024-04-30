using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.utils.editor
{
    using fwp.appendix.user;
    using UnityEditor;

    /// <summary>
    /// PROVIDE:
    /// tabs for refreshable window
    /// 
    /// usage:
    /// override generate methods to feed your own content
    /// </summary>
    abstract public class WinEdTabs : WinEdRefreshable
    {
        const string _editor__profiler_tab = "tab_scene_profiler";

        public class WinTabState
        {
            public string path;

            /// <summary>
            /// only use for display
            /// </summary>
            public string label => path.Substring(path.LastIndexOf("/") + 1);

            /// <summary>
            /// how to draw content of this tab
            /// </summary>
            public System.Action drawCallback;

            /// <summary>
            /// scroll value
            /// </summary>
            public Vector2 scroll;
        }

        public class WinTabsState
        {
            const int defaultTab = 0;

            public int tabActive
            {
                get => MgrUserSettings.getEdInt(ppUID, defaultTab);
                set
                {
                    MgrUserSettings.setEdInt(ppUID, value);
                    //Debug.Log(uid+"?"+value);
                }
            }

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

            string ppUID => _editor__profiler_tab + "_" + GetType() + "_" + tUID;

            string tUID;

            public WinTabsState(string uid)
            {
                this.tUID = uid;
            }

            public string getUid() => tUID;
        }

        WinTabsState stateEditime;
        WinTabsState stateRuntime;

        protected WinTabsState tabsState => Application.isPlaying ? stateRuntime : stateEditime;

        /// <summary>
        /// what tabs to draw !runtime
        /// tab label, gui draw callback
        /// return : true to draw additionnal content
        /// </summary>
        abstract public (string, System.Action)[] generateTabsEditor();

        /// <summary>
        /// what to draw @runtime
        /// default is same as edit time
        /// return null : to draw nothing
        /// func return true : to draw additionnal content
        /// </summary>
        virtual public (string, System.Action)[] generateTabsRuntime()
            => new (string, System.Action)[0];

        public void selectDefaultTab()
        {
            tabsState.tabActive = 0;
        }

        public void selectTab(int index) => tabsState.tabActive = index;

        protected override void reactPlayModeState(PlayModeStateChange state)
        {
            base.reactPlayModeState(state);

            //case PlayModeStateChange.ExitingPlayMode:
            //case PlayModeStateChange.EnteredEditMode:

        }

        override public void refresh(bool force = false)
        {
            base.refresh(force);

            if (force || stateEditime == null || stateEditime.tabsContent.Length <= 0)
            {
                var data = generateTabsEditor();
                stateEditime = generateState("editor", data);

                log("refresh-ed editor tabs (x" + stateEditime.tabs.Count + ")");

                stateRuntime = null;
                data = generateTabsRuntime();
                if (data != null)
                {
                    if (data.Length > 0)
                    {
                        stateRuntime = generateState("runtime", data);
                        log("refresh-ed runtime tabs (x" + stateRuntime.tabs.Count + ")");
                    }
                    else
                    {
                        stateRuntime = stateEditime;
                    }
                }
            }

            if (force)
            {
                //DONT it will reset tab at every compilation
                //selectDefaultTab();
            }
        }

        WinTabsState generateState(string uid, (string, System.Action)[] data)
        {
            WinTabsState state = new WinTabsState(uid);

            foreach (var tabTuple in data)
            {
                var tab = new WinTabState();
                tab.path = tabTuple.Item1;

                tab.drawCallback = tabTuple.Item2;

                if (state.tabs == null)
                    state.tabs = new List<WinTabState>();

                state.tabs.Add(tab);

                log("added tab -> " + tab.label);
            }

            // store stuff for unity drawing
            state.tabsContent = TabsHelper.generateTabsDatas(state.labels.ToArray());

            return state;
        }

        sealed protected override void draw()
        {
            base.draw();

            var _state = tabsState;

            if (_state == null)
                return;

            int currTabIndex = _state.tabActive;

            if (_state == null)
            {
                GUILayout.Label("no tabs available");
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

                if (_tabIndex >= _state.tabs.Count)
                {
                    if (verbose)
                        Debug.LogWarning(_tabIndex + " oob ? " + _state.tabs.Count);

                    selectTab(0);

                    return;
                }

                var tab = _state.tabs[_tabIndex];

                drawFilterField();

                GUILayout.Space(15f);

                if (tab.drawCallback != null)
                {
                    tab.scroll = GUILayout.BeginScrollView(tab.scroll);
                    //Debug.Log(tab.scroll);

                    // draw gui
                    tab.drawCallback.Invoke();

                    GUILayout.EndScrollView();
                }

            }

        }

        /// <summary>
        /// shortcut to draw a tab header
        /// </summary>
        public int drawTabsHeader(int tabSelected, GUIContent[] tabs)
        {
            //GUIStyle gs = new GUIStyle(GUI.skin.button)
            //int newTab = GUILayout.Toolbar((int)tabSelected, modeLabels, "LargeButton", GUILayout.Width(toolbarWidth), GUILayout.ExpandWidth(true));
            int newTab = GUILayout.Toolbar((int)tabSelected, tabs, "LargeButton");
            //if (newTab != (int)tabSelected) Debug.Log("changed tab ? " + tabSelected);

            return newTab;
        }

    }

}
