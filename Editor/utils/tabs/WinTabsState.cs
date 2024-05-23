using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.utils.editor
{
    using fwp.appendix.user;

    /// <summary>
    /// wrapper that keeps all data of different tabs
    /// </summary>
    public class WrapperTabs
    {
        const string _editor__profiler_tab = "tab_scene_profiler";

        public int tabActive
        {
            get => MgrUserSettings.getEdInt(ppUID, 0);
            set
            {
                MgrUserSettings.setEdInt(ppUID, Mathf.Clamp(value, 0, tabs.Count - 1));
                //Debug.Log(uid+"?"+value);
            }
        }

        public bool isSetup => tabsContent.Length > 0;
        public int countTabs => tabs.Count;

        List<WrapperTab> tabs = new List<WrapperTab>();

        // util for unity drawing
        GUIContent[] tabsContent = new GUIContent[0];

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

        public List<string> paths
        {
            get
            {
                List<string> output = new List<string>();
                foreach (var tab in tabs)
                {
                    output.Add(tab.path);
                }
                return output;
            }

        }

        string ppUID => _editor__profiler_tab + "_" + wuid;

        string wuid;

        public WrapperTabs(string uid)
        {
            wuid = uid;
        }

        public void selectDefaultTab() => tabActive = 0;

        public string getWrapperUid() => wuid;

        /// <summary>
        /// add various tabs to wrapper
        /// </summary>
        public void add(string path, System.Action draw)
        {
            WrapperTab wts = new WrapperTab();
            wts.path = path;
            wts.drawCallback = draw;
            tabs.Add(wts);

            // store stuff for unity drawing
            tabsContent = TabsHelper.generateTabsDatas(labels.ToArray());
        }

        /// <summary>
        /// shortcut to draw a tab header
        /// </summary>
        public bool drawTabsHeader()
        {

            //GUIStyle gs = new GUIStyle(GUI.skin.button)
            //int newTab = GUILayout.Toolbar((int)tabSelected, modeLabels, "LargeButton", GUILayout.Width(toolbarWidth), GUILayout.ExpandWidth(true));
            int newTab = GUILayout.Toolbar(tabActive, tabsContent, "LargeButton");
            //if (newTab != (int)tabSelected) Debug.Log("changed tab ? " + tabSelected);

            if(newTab != tabActive)
            {
                if (newTab >= tabsContent.Length) newTab = tabsContent.Length - 1;
                if (newTab < 0) newTab = 0;

                tabActive = newTab;
                return true;
            }

            return false;
        }

        public void drawActiveTab()
        {
            tabs[tabActive].draw();
        }

        public WrapperTab getActiveTab() => tabs[tabActive];
    }


    /// <summary>
    /// wrapper for one tab
    /// </summary>
    public class WrapperTab
    {
        /// <summary>
        /// complete path to section
        /// </summary>
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

        public void draw()
        {
            if (drawCallback == null)
                return;

            scroll = GUILayout.BeginScrollView(scroll);

            // draw gui
            drawCallback.Invoke();

            GUILayout.EndScrollView();
        }
    }


}
