using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.utils.editor
{
    using fwp.appendix.user;

    /// <summary>
    /// wrapper that keeps all data of different tabs
    /// </summary>
    public class WinTabsState
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

        string ppUID => _editor__profiler_tab + "_" + tUID;

        string tUID;

        public WinTabsState(string uid)
        {
            this.tUID = uid;
        }

        public string getUid() => tUID;
    }



    public class WinTabState
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
