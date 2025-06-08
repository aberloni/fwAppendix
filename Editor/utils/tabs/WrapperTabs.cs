using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.utils.editor.tabs
{
    using fwp.appendix.user;
	using fwp.settings.editor;
	using System;

    /// <summary>
    /// wrapper that keeps all data of different tabs
    /// </summary>
    public class WrapperTabs : iTab
    {
        const string _editor__profiler_tab = "tab_scene_profiler";

        public int tabActive
        {
            get
            {
                int idx = MgrEdUserSettings.getInt(ppUID, 0);

                idx = Mathf.Max(0, idx);
                if (tabs != null && tabs.Count > 0) idx = Mathf.Min(idx, tabs.Count - 1);

                return idx;
            }
            set
            {
                int idx = value;

                idx = Mathf.Max(0, idx);
                if (tabs != null && tabs.Count > 0) idx = Mathf.Min(idx, tabs.Count - 1);

                MgrEdUserSettings.setInt(ppUID, idx);
                //Debug.Log(uid+"?"+value);
            }
        }

        public bool isSetup => tabsContent.Length > 0;
        public int countTabs => tabs.Count;

        public iTab getActiveTab() => tabs[tabActive];
        public iTab getTabByIndex(int idx) => tabs[idx];

        List<iTab> tabs = new List<iTab>();

        // util for unity drawing
        GUIContent[] tabsContent = new GUIContent[0];

        public System.Action<iTab> onTabChanged;

        public List<string> labels
        {
            get
            {
                List<string> output = new List<string>();
                foreach (var tab in tabs)
                {
                    output.Add(tab.GetTabLabel());
                }
                return output;
            }

        }

        string ppUID => _editor__profiler_tab + "_" + wrapperUID;

        public string GetTabLabel()
        {
            if (!string.IsNullOrEmpty(label)) return label;
            return wrapperUID;
        }

        /// <summary>
        /// ppref identifier
        /// </summary>
        string wrapperUID = string.Empty;
        public string getWrapperUid() => wrapperUID;

        string label = string.Empty;

        public void setContainerLabel(string label)
        {
            this.label = label;
        }

        /// <summary>
        /// wuid : ppref identifier
        /// </summary>
        public WrapperTabs(string wuid)
        {
            wrapperUID = wuid;
        }

        virtual public void Refresh()
        { }

        public void selectDefaultTab() => tabActive = 0;

        public void addSpecificTab(iTab tab)
        {
            tabs.Add(tab);

            // store stuff for unity drawing
            tabsContent = TabsHelper.generateTabsDatas(labels.ToArray());
        }

        /// <summary>
        /// add various tabs to wrapper
        /// draw callback will receive path as parameter
        /// </summary>
        public WrapperTab addGenericTab(string label, System.Action draw = null)
        {
            WrapperTab wt = new WrapperTab(label, draw);

            addSpecificTab(wt);

            return wt;
        }

        /// <summary>
        /// shortcut to draw a tab header
        /// </summary>
        void drawTabsHeader()
        {
            //GUIStyle gs = new GUIStyle(GUI.skin.button)
            //int newTab = GUILayout.Toolbar((int)tabSelected, modeLabels, "LargeButton", GUILayout.Width(toolbarWidth), GUILayout.ExpandWidth(true));
            int newTab = GUILayout.Toolbar(tabActive, tabsContent, "LargeButton");
            //if (newTab != (int)tabSelected) Debug.Log("changed tab ? " + tabSelected);

            if (newTab != tabActive)
            {
                if (newTab >= tabsContent.Length) newTab = tabsContent.Length - 1;
                if (newTab < 0) newTab = 0;

                tabActive = newTab;

                var t = getActiveTab();
                if(t != null)
                {
                    t.Refresh();
                }

                onTabChanged?.Invoke(t);
            }
        }

        virtual public void Draw()
        {
            drawTabsHeader();
            var t = getActiveTab();
            if (t != null) t.Draw();
        }
    }



}
