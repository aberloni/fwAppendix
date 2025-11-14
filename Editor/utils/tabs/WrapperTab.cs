using UnityEngine;
using UnityEditor;

namespace fwp.utils.editor.tabs
{
	/// <summary>
	/// for nested tabs
	/// </summary>
	public interface iTab
	{
		/// <summary>
		/// refresh some content
		/// also called on selection
		/// </summary>
		public void Refresh(bool force);

		/// <summary>
		/// tab label display
		/// </summary>
		public string GetTabLabel();

		/// <summary>
		/// what to draw when selected
		/// </summary>
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

		virtual public void Refresh(bool force)
		{ }

		/// <summary>
		/// how to draw content of this tab
		/// param is parent EditorWindow
		/// </summary>
		System.Action drawCallback = null;

		/// <summary>
		/// scroll value
		/// </summary>
		Vector2 tabScroll;

		protected WinEdTabs owner;

		public WrapperTab(WinEdTabs window = null)
		{
			owner = window;

			label = string.Empty;
			drawCallback = null;
		}

		/// <summary>
		/// drawGUI = additionnal drawing on top of native draw
		/// </summary>
		public WrapperTab(string label, WinEdTabs window = null, System.Action drawGUI = null)
		{
			owner = window;

			this.label = label;
			this.drawCallback = drawGUI;
		}

		public void Draw()
		{
			tabScroll = GUILayout.BeginScrollView(tabScroll);

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
