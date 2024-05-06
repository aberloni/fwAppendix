using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.utils.editor
{
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

        public void resetTabSelection()
        {
            tabsState.tabActive = 0;
        }
        public void selectTab(int index)
        {
            tabsState.tabActive = index;
        }

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
                stateEditime = generateState("editor-" + GetType(), data);

                log("refresh-ed editor tabs (x" + stateEditime.tabs.Count + ")");

                stateRuntime = null;
                data = generateTabsRuntime();
                if (data != null)
                {
                    if (data.Length > 0)
                    {
                        stateRuntime = generateState("runtime-" + GetType(), data);
                        log("refresh-ed runtime tabs (x" + stateRuntime.tabs.Count + ")");
                    }
                    else
                    {
                        stateRuntime = stateEditime;
                    }
                }
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
            {
                GUILayout.Label("no tabs available");
                return;
            }

            drawFilterField();

            GUILayout.Space(15f);

            // draw labels buttons
            // +oob check
            var _tabIndex = drawTabsHeader(_state.tabActive, _state.tabsContent);

            bool tabChanged = _tabIndex != _state.tabActive;

            // selection changed ?
            if (tabChanged)
            {
                //assign
                _state.tabActive = _tabIndex; // +oob check

                //verbose = true;
                onTabChanged(_tabIndex);
            }

            var tab = _state.tabs[_tabIndex];
            tab?.draw();
        }

        virtual protected void onTabChanged(int tab)
        {
            if(verbose) Debug.Log("selected tab #" + tab + " @ " + tabsState.tabs[tab].path);
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

            if (newTab >= tabs.Length) newTab = tabs.Length - 1;
            if (newTab < 0) newTab = 0;

            return newTab;
        }

    }

}
