using UnityEngine;

namespace fwp.utils.editor.tabs
{
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

        /// <summary>
        /// owner
        /// </summary>
        protected WinEdTabs window;

        public WrapperTab(WinEdTabs window)
        {
            this.window = window;
        }

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
