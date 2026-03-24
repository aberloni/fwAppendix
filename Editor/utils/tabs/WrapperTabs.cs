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

		public int TabActiveIndex
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

		string ppUID => _editor__profiler_tab + "_" + wrapperUID;

		public bool IsSetup => tabsContent.Length > 0;
		public int CountTabs => tabs.Count;

		public iTab getActiveTab()
		{
			if (tabs == null || tabs.Count <= 0) return null;
			TabActiveIndex = Mathf.Clamp(TabActiveIndex, 0, tabs.Count - 1);
			return tabs[TabActiveIndex].tab;
		}

		virtual public bool IsAvailable() => true;

		public iTab getTabByIndex(int idx) => tabs[idx].tab;

		struct TabContent
		{
			public iTab tab;
			public GUIContent content;
		}

		List<TabContent> tabs = new();

		// util for unity drawing
		GUIContent[] tabsContent = new GUIContent[0];

		/// <summary>
		/// for external content reaction
		/// prefer using local virtual method
		/// </summary>
		public System.Action<iTab> onTabChanged;

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

		public void selectDefaultTab() => TabActiveIndex = 0;

		virtual public bool hasContentToDraw()
		{
			return tabs.Count > 0;
		}

		GUIContent[] generateContents()
		{
			var ret = new List<GUIContent>();
			foreach (var t in tabs)
			{
				if (!t.tab.IsAvailable()) continue;
				ret.Add(t.content);
			}
			return ret.ToArray();
		}

		public void addSpecificTab(iTab tab)
		{
			tabs.Add(new TabContent()
			{
				tab = tab,
				content = new GUIContent(tab.GetTabLabel())
			});

			tabsContent = generateContents();

			owner?.refresh();
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


		const string buttonLarge = "LargeButton";

		/// <summary>
		/// line of tabs
		/// </summary>
		virtual protected void drawTabsLine()
		{
			//GUIStyle gs = new GUIStyle(GUI.skin.button)
			//int newTab = GUILayout.Toolbar((int)tabSelected, modeLabels, "LargeButton", GUILayout.Width(toolbarWidth), GUILayout.ExpandWidth(true));
			int newTab = GUILayout.Toolbar(TabActiveIndex, tabsContent, buttonLarge);
			//if (newTab != (int)tabSelected) Debug.Log("changed tab ? " + tabSelected);

			if (newTab != TabActiveIndex)
			{
				if (newTab >= tabsContent.Length) newTab = tabsContent.Length - 1;
				if (newTab < 0) newTab = 0;

				TabActiveIndex = newTab;

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
			if (tab != null) tab.Refresh(false); // soft refresh on tab swap

			onTabChanged?.Invoke(tab);
		}

	}



}
