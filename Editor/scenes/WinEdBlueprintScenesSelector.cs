using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.scenes.editor
{
	using fwp.utils.editor.tabs;
	using fwp.scenes;
	using UnityEngine.Analytics;

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
		/// <summary>
		/// assoc btw tab label and some sub bolbs
		/// tab label
		/// sub folder scene profiles[]
		/// </summary>
		Dictionary<string, List<SceneSubFolder>> sections = null;

		public Dictionary<string, List<SceneSubFolder>> Sections
		{
			get
			{
				if (sections == null) sections = new();
				return sections;
			}
		}

		public bool HasSections => sections != null && sections.Count > 0;

		virtual protected bool useProgressBar() => true;

		/// <summary>
		/// can be replaced by different way to handle scene profil
		/// </summary>
		virtual protected SceneProfil generateProfil(string uid)
		{
			// log("profil.generate: " + uid);
			return new SceneProfil(uid);
		}

		/// <summary>
		/// can be replaced by different way to hande subs
		/// </summary>
		virtual protected SceneSubFolder generateSub(string profilUid)
		{
			return new SceneSubFolder(profilUid);
		}

		protected override void onTabChanged(iTab tab)
		{
			base.onTabChanged(tab);

			if (tab is TabSceneSelector tss)
			{
				injectSubSection(tss.Path); // tab change, reeval tab content
			}
		}

		public override void refresh(bool force = false)
		{
			if (force) SceneTools.dirtyScenePath();

			base.refresh(force);

			if (force)
			{
				if (sections == null) sections = new();
				sections.Clear();
			}

			var state = ActiveTabs; // getter edit/runtime tabs

			if (state != null || force) // ed/run tabs
			{
				injectSubSectionOfActiveTab(state);
			}
		}

		void refreshSections()
		{
			if (HasSections)
			{
				foreach (var section in sections)
				{
					if (section.Value == null) continue;

					log("section.refresh." + section.Key + "=" + section.Value);

					foreach (var folder in section.Value)
					{
						foreach (var profil in folder.profils)
						{
							log("profil.refresh." + profil.label);
							profil.refresh();
						}
					}
				}
			}
		}

		/// <summary>
		/// inject all tabs path to sub sections
		/// </summary>
		void injectSubSectionOfActiveTab(WrapperTabs state)
		{
			if (state == null) return; // no tabs injected

			var cTab = state.getActiveTab();

			if (cTab == null) return; // no active tab

			if (cTab is TabSceneSelector tss)
			{
				injectSubSection(tss.Path);
			}
			else Debug.LogWarning($"injection issue ? injected tab {cTab} is not a <TabSceneSelector>");
		}

		void injectSubSection(string sectionPath)
		{
			//if (verbose) Debug.Log("SceneSelector :: refresh section : " + sectionPath);
			if (string.IsNullOrEmpty(sectionPath))
				return;

			if (sections == null) sections = new();

			// remove if previous
			if (!sections.ContainsKey(sectionPath))
			{
				sections.Add(sectionPath, null);
			}

			List<SceneSubFolder> tabContent = solveTabFolder(sectionPath);

			if (tabContent == null) sections.Remove(sectionPath);
			else
			{
				sections[sectionPath] = tabContent;
				log("section.update: " + sectionPath);
			}

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

		List<SceneSubFolder> solveTabFolder(string tabName)
		{
			List<SceneProfil> profils = getProfils(tabName);
			if (profils == null)
			{
				Debug.LogError("null profils while solving tabs ?");
				return null;
			}

			Dictionary<string, List<SceneProfil>> list = new Dictionary<string, List<SceneProfil>>();

			log("folder/sorting x" + profils.Count + " profiles");

			// all profil will be matched based on the parent path
			foreach (SceneProfil profil in profils)
			{
				string parent = profil.ParentPath;

				//Debug.Log(profil.label + " @ " + profil.parentPath);

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

				sub.profils = kp.Value;

				log(sub.stringify());

				output.Add(sub);
			}

			log("folder/solved x" + output.Count + " subs");

			return output;
		}

		/// <summary>
		/// g�n�re tout les profiles qui sont de la categorie
		/// </summary>
		protected List<SceneProfil> getProfils(string category)
		{
			List<SceneProfil> profils = new();

			SceneProfil.verbose = verbose;

			// works with Contains
			var cat_paths = SceneTools.getScenesPathsOfCategory(category, true);

			// list of contexts (scenes base_name, without layers)
			// keep only scene base_name
			List<string> contexts = new();
			for (int i = 0; i < cat_paths.Count; i++)
			{
				string p = cat_paths[i];
				string context = SceneProfil.extractContextFromPath(SceneTools.removePathBeforeFile(p));

				if (contexts.Contains(context)) continue;
				contexts.Add(context);
			}

			log("getProfils() category: <b>" + category + "</b> -> total scenes x" + cat_paths.Count + " & total contexts x" + contexts.Count);

			for (int i = 0; i < contexts.Count; i++)
			{
				string ctx = contexts[i];

#if UNITY_EDITOR
				if (useProgressBar())
				{
					float progr = (i * 1f) / (contexts.Count * 1f);
					if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(
						"generateProfil profil : " + category,
						"path: " + ctx, progr))
					{
						return null; // cancelled
					}
				}
#endif

				// generate a profil with given path
				var sp = generateProfil(ctx);

				// check if the profil is already part of profils[]
				if (!sp.HasLayers)
				{
					log(ctx + " has no content, don't keep this profil");
					continue;
				}

				profils.Add(sp);
				log("+PROFIL label:" + sp.label + " (lyr x" + sp.layers.Count + ") @ " + ctx);
			}

			log("total profils solved x" + profils.Count);
			foreach (var p in profils) log(p.stringify());

#if UNITY_EDITOR
			if (useProgressBar())
			{
				UnityEditor.EditorUtility.ClearProgressBar();
			}
#endif

			return profils;
		}


		public SceneProfil getOpenedProfil()
		{
			TabSceneSelector tss = ActiveTabs.getActiveTab() as TabSceneSelector;
			var category = sections[tss.Path];

			foreach (var profil in category)
			{
				foreach (var sp in profil.profils)
				{
					if (sp.isLoaded()) return sp;
				}
			}

			return null;
		}

		public void selectFolder(string path, bool unfold = false) => fwp.utils.editor.GuiHelpers.selectFolder(path, unfold);
	}

}