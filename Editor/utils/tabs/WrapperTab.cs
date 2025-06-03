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
        System.Action drawCallback;

        /// <summary>
        /// scroll value
        /// </summary>
        Vector2 scroll;

        /// <summary>
        /// to acces owner
        /// parent editor window
        /// </summary>
        EditorWindow _window;

        public EditorWindow Parent => _window;

        public WrapperTab(UnityEditor.EditorWindow window, string label = null, System.Action drawGUI = null)
        {
            _window = window;
            if (label != null) this.label = label;
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
