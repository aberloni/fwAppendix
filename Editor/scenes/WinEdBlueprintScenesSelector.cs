using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.scenes.editor
{
	using fwp.utils.editor.tabs;
	using fwp.scenes;
	using UnityEngine.Analytics;
    using NUnit.Framework;

    /// <summary>
    /// 
    /// meant to :
    /// give a list of folder to target (tab names)
    /// search within folder all scenes
    /// separate scenes with same parent folder
    /// 
    /// setup:
    /// - provide TabSceneSelector(s) in populateTabsEditor
    /// 
    /// possible : 
    /// - override SceneProfil
    /// - override SceneSubFolder
    /// - override footer
    /// - override getWindowTabName
    /// 
    /// </summary>
    abstract public class WinEdBlueprintScenesSelector : WinEdTabs
	{
		protected override void onTabChanged(iTab tab)
		{
			base.onTabChanged(tab);

			log("tab.changed : "+tab);

			if (tab is TabSceneSelector tss)
			{
				tss.verbose = verbose;
				tss.Refresh(false); // tab change, reeval tab content
			}
		}

		public override void refresh(bool force = false)
		{
			if (force) SceneTools.dirtyScenePath();

			base.refresh(force);

			if (force) // ed/run tabs
			{
				var state = ActiveTabs; // getter edit/runtime tabs
				state.Refresh(force);
			}
			
			ActiveTabs.getActiveTab()?.Refresh(force);
		}

		/// <summary>
		/// additionnal stuff under tabs zone
		/// </summary>
		protected override void drawFooter()
		{
			base.drawFooter();

			settings.utils.UtilEdUserSettings.drawBool(
				"+build settings", SceneSubFolder._pref_autoAddBuildSettings, (state) => primeRefresh());
		}

		public void selectFolder(string path, bool unfold = false) => fwp.utils.editor.GuiHelpers.selectFolder(path, unfold);
	}

}