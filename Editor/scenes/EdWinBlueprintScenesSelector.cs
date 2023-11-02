using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;

using UnityEditor;
using UnityEditor.SceneManagement;

namespace fwp.scenes
{
	using fwp.appendix;
	using fwp.appendix.user;

	/// <summary>
	/// 
	/// FEED:
	/// base pathsub section
	/// 
	/// PROVIDE:
	/// buttons to open SceneProfil
	/// 
	/// give a list of folder to target (tab names)
	/// search within folder all scenes
	/// separate scenes with same parent folder
	/// 
	/// how to use :
	/// - inherite of this class to have your own window
	/// - implement sections names for tabs
	/// - you can override generateProfil to use some specific SceneProfil
	/// </summary>
	abstract public class EdWinBlueprintScenesSelector : EdWinTabs
	{
		/// <summary>
		/// assoc btw tab label and some sub bolbs
		/// tab label
		/// sub folder scene profiles[]
		/// </summary>
		Dictionary<string, List<SceneSubFolder>> sections = null;

		/// <summary>
		/// can be replaced by different way to handle scene profil
		/// </summary>
        virtual protected SceneProfil generateProfil(string uid)
		{
			return new SceneProfil(uid);
		}

		/// <summary>
		/// can be replaced by different way to hande subs
		/// </summary>
		virtual protected SceneSubFolder generateSub(string folder)
		{
			return new SceneSubFolder(rootPath(), folder);
		}

        /// <summary>
        /// path/to/tabs folder
        /// </summary>
        virtual protected string rootPath() => string.Empty;

		protected override void refresh(bool force = false)
		{
			base.refresh(force);

			if (force)
				Debug.Log(GetType() + " force refreshing content");

			var state = tabsState;

            if (state != null && sections == null || force)
			{
                sections = new Dictionary<string, List<SceneSubFolder>>();
                injectSubSections(state);
			}

		}

		void injectSubSections(WinTabsState state)
		{
            // each possible labels into sub folder blob
            for (int i = 0; i < state.tabs.Count; i++)
            {
                var lbl = state.tabs[i].label;
                List<SceneSubFolder> tabContent = solveTabFolder(lbl);
                sections.Add(lbl, tabContent);
            }

            if (verbose)
            {
                Debug.Log("sub folder sections x" + sections.Count);
            }

        }

        protected override void updateEditime()
        {
            base.updateEditime();

			if (sections == null)
				refresh();
		}

		protected bool drawSubs(string tabLabel)
		{
			var subList = sections[tabLabel];

			GUILayout.BeginHorizontal();

			GUILayout.Label($"{tabLabel} has x{subList.Count} sub-sections");

			if (GUILayout.Button("ping folder"))
			{
				pingFolder(Path.Combine(rootPath(), tabLabel));
			}

			GUILayout.EndHorizontal();

			for (int i = 0; i < subList.Count; i++)
			{
				subList[i].drawSection(filter);
			}

			return false;
		}

        /// <summary>
        /// additionnal stuff under tabs zone
        /// </summary>
        protected override void drawAdditionnal()
        {
            base.drawAdditionnal();

			SceneSubFolder.drawAutoAdd();
        }

		List<SceneSubFolder> solveTabFolder(string tabName)
		{
			List<SceneProfil> profils = getProfils(tabName);

			Dictionary<string, List<SceneProfil>> list = new Dictionary<string, List<SceneProfil>>();

			//Debug.Log("sorting x" + profils.Count + " profiles");

			foreach (SceneProfil profil in profils)
			{
				string parent = profil.parentFolder;

				if (!list.ContainsKey(parent))
				{
					//Debug.Log("added " + parent);
					list.Add(parent, new List<SceneProfil>());
				}
				list[parent].Add(profil);
			}

			List<SceneSubFolder> output = new List<SceneSubFolder>();

			foreach (var kp in list)
			{
				SceneSubFolder sub = generateSub(kp.Key);

				sub.scenes = kp.Value;

				output.Add(sub);
			}

			//Debug.Log("solved x" + output.Count + " subs");

			return output;
		}

		/// <summary>
		/// génère tout les profiles qui sont de la categorie
		/// </summary>
		protected List<SceneProfil> getProfils(string category)
		{
			List<SceneProfil> profils = new List<SceneProfil>();

			// works with Contains
			var cat_paths = SceneTools.getScenesPathsOfCategory(category, true);

			if (verbose)
				Debug.Log("category:" + category + " paths x" + cat_paths.Count);

			foreach (string path in cat_paths)
			{
				SceneProfil sp = generateProfil(path);
				if (sp.isValid())
				{
					bool found = false;

					foreach (var profil in profils)
					{
						if (profil.match(sp))
							found = true;
					}

					if (!found)
					{
						profils.Add(sp);

						if (verbose)
							Debug.Log(sp.uid);
					}

				}
			}

			if (verbose)
				Debug.Log("solved x" + profils.Count + " profiles");

			return profils;
		}

		public SceneProfil getOpenedProfil()
		{
			var category = sections[tabsState.tabs[tabActive].label];

			foreach (var profil in category)
			{
				foreach (var sp in profil.scenes)
				{
					if (sp.isLoaded()) return sp;
				}
			}

			return null;
		}

	}

}