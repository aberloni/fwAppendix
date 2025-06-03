using UnityEngine;
using UnityEditor;

namespace fwp.utils.editor.tabs
{
    /// <summary>
    /// for nested tabs
    /// </summary>
    public interface iTab
    {
        public string GetTabLabel();
        public void Draw();
    }

    /// <summary>
    /// wrapper for one tab
    /// </summary>
    public class WrapperTab : iTab
    {
        /// <summary>
        /// complete path to section
        /// </summary>
        protected string label = string.Empty;

        public string GetTabLabel() => label;

        /// <summary>
        /// how to draw content of this tab
        /// param is parent EditorWindow
        /// </summary>
        System.Action drawCallback = null;

        /// <summary>
        /// scroll value
        /// </summary>
        Vector2 scroll;

        public WrapperTab()
        {
            label = string.Empty;
            drawCallback = null;
        }

        public WrapperTab(string label, System.Action drawGUI = null)
        {
            this.label = label;
            this.drawCallback = drawGUI;
        }

        public void Draw()
        {
            scroll = GUILayout.BeginScrollView(scroll);

            drawGUI();

            // to replace drawGUI an inheritence flow
            // or draw additionnal external content
            if (drawCallback != null)
            {
                drawCallback?.Invoke();
            }

            GUILayout.EndScrollView();
        }

        /// <summary>
        /// what to draw
        /// </summary>
        virtual protected void drawGUI()
        { }
    }

}
