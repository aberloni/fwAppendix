using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.utils.editor.tabs
{
	using fwp.settings.editor;

	/// <summary>
	/// wrapper that keeps all data of different tabs
	/// use add methods to add tabs to this tab
	/// </summary>
	public class WrapperTabs : iTab
	{
		const string _editor__profiler_tab = "tab_scene_profiler";

		public int tabActiveIndex
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

		public iTab getActiveTab()
		{
			if (tabs == null || tabs.Count <= 0) return null;
			tabActiveIndex = Mathf.Clamp(tabActiveIndex, 0, tabs.Count - 1);
			return tabs[tabActiveIndex];
		}

		public iTab getTabByIndex(int idx) => tabs[idx];

		List<iTab> tabs = new List<iTab>();

		// util for unity drawing
		GUIContent[] tabsContent = new GUIContent[0];

		/// <summary>
		/// for external content reaction
		/// prefer using local virtual method
		/// </summary>
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

		protected WinEdTabs owner;

		/// <summary>
		/// wuid : ppref identifier
		/// </summary>
		public WrapperTabs(string wuid, WinEdTabs window = null)
		{
			wrapperUID = wuid;
			owner = window;

			if (window != null)
			{
				window.onFilterValueChanged += onFilterValueChanged;
			}
		}

		void onFilterValueChanged(string val)
		{ }

		/// <summary>
		/// called on tab change
		/// or header refresh
		/// </summary>
		virtual public void Refresh(bool force)
		{ }

		public void selectDefaultTab() => tabActiveIndex = 0;

		virtual public bool hasContentToDraw()
		{
			return tabs.Count > 0;
		}

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
		public WrapperTab addGenericTab(string label, WinEdTabs window = null, System.Action draw = null)
		{
			WrapperTab wt = new WrapperTab(label, window, draw);

			addSpecificTab(wt);

			return wt;
		}

		/// <summary>
		/// content before line of tabs
		/// </summary>
		virtual protected void drawTabsHeader()
		{ }

		/// <summary>
		/// line of tabs
		/// </summary>
		virtual protected void drawTabsLine()
		{
			//GUIStyle gs = new GUIStyle(GUI.skin.button)
			//int newTab = GUILayout.Toolbar((int)tabSelected, modeLabels, "LargeButton", GUILayout.Width(toolbarWidth), GUILayout.ExpandWidth(true));
			int newTab = GUILayout.Toolbar(tabActiveIndex, tabsContent, "LargeButton");
			//if (newTab != (int)tabSelected) Debug.Log("changed tab ? " + tabSelected);

			if (newTab != tabActiveIndex)
			{
				if (newTab >= tabsContent.Length) newTab = tabsContent.Length - 1;
				if (newTab < 0) newTab = 0;

				tabActiveIndex = newTab;

				reactTabChanged(getActiveTab());
			}
		}

		virtual public void Draw()
		{
			// stuff above tabs line
			drawTabsHeader();

			// lock ?
			if (!hasContentToDraw())
			{
				GUILayout.Label("nothing here");
			}
			else
			{
				//tabs line
				drawTabsLine();

				// active tab
				var t = getActiveTab();
				if (t != null) t.Draw();
			}
		}

		/// <summary>
		/// will also refresh the tab
		/// </summary>
		virtual protected void reactTabChanged(iTab tab)
		{
			if (tab != null)
			{
				tab.Refresh(true);
			}

			onTabChanged?.Invoke(tab);
		}
	}



}
