using UnityEngine;
using System.Collections.Generic;


namespace fwp.scenes.editor
{
	using fwp.utils.editor.tabs;
	using fwp.utils.editor;

	/// <summary>
	/// tab
	/// </summary>
	public class TabSceneSelector : WrapperTab
	{
		string path;

		public string Path => path;

		public string PathEnd => path.Substring(path.LastIndexOf("/") + 1);

		WinEdBlueprintScenesSelector selector;

		static readonly GUIContent lblPingFolder = new GUIContent("ping folder");
		static readonly GUIContent lblUpfoldAll = new GUIContent("upfold all");
		static readonly GUIContent lblEmpty = new GUIContent("selector has no sections");

		public TabSceneSelector(WinEdBlueprintScenesSelector window, string path) : base()
		{
			selector = window;

			// remove last "/"
			this.path = path.EndsWith("/") ? path.Substring(0, path.LastIndexOf("/") - 1) : path;

			setLabel(PathEnd); // autolabel
		}

		protected override void drawGUI()
		{
			base.drawGUI();
			drawSubSectionTab();
		}

		/// <summary>
		/// draw generic tab with scene list
		/// </summary>
		void drawSubSectionTab()
		{
			string subSectionUid = Path;

			if (!selector.HasSections)
			{
				GUILayout.Label(lblEmpty);
				return;
			}

			var sections = selector.Sections;

			List<SceneSubFolder> subList = null;

			if (sections.ContainsKey(subSectionUid))
			{
				subList = sections[subSectionUid];
			}

			if (subList == null)
			{
				GUILayout.Label("no sublist #" + subSectionUid);
				return;
			}

			GUILayout.BeginHorizontal();
			
			GUILayout.Label($"{subSectionUid} has x{subList.Count} sub-sections");

			if (GUILayout.Button(lblPingFolder, GUILayout.Width(QuickEditorViewStyles.btnL)))
			{
				GuiHelpers.selectFolder(subSectionUid, true);
			}

			if (GUILayout.Button(lblUpfoldAll, GUILayout.Width(QuickEditorViewStyles.btnL)))
			{
				for (int i = 0; i < subList.Count; i++)
				{
					subList[i].Toggled = false;
				}
			}

			GUILayout.EndHorizontal();

			for (int i = 0; i < subList.Count; i++)
			{
				subList[i].drawSection(selector.Filter);
			}

		}

	}

}