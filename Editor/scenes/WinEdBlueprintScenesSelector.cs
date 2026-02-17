using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.scenes.editor
{
	using fwp.utils.editor.tabs;
	using fwp.scenes;

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
			//Debug.Log("generating default profil : " + uid);
			return new SceneProfil(uid);
		}

		/// <summary>
		/// can be replaced by different way to hande subs
		/// </summary>
		virtual protected SceneSubFolder generateSub(string profilUid)
		{
			return new SceneSubFolder(profilUid);
		}

		void onTabChanged(iTab tab)
		{
			log("tab changed    => <b>" + tab.GetTabLabel() + "</b>");

			TabSceneSelector tss = ActiveTabs.getActiveTab() as TabSceneSelector;

			injectSubSection(tss.Path); // tab change, reeval tab content
		}

		public override void refresh(bool force = false)
		{
			if(force) SceneTools.dirtyScenePath();

			base.refresh(force);

			var state = ActiveTabs; // getter edit/runtime tabs

			if (state != null) // ed/run tabs
			{
				if (!HasSections || force)
				{
					if (sections == null) sections = new();
					else sections.Clear();

					injectSubSections(state);
				}

				if (force)
				{
					state.onTabChanged = null;
					state.onTabChanged += onTabChanged;
				}
			}

			if (HasSections || force)
			{
				foreach (var section in sections)
				{
					if (section.Value == null) continue;

					foreach (var folder in section.Value)
					{
						foreach (var profil in folder.profils)
						{
							profil.refresh();
						}
					}
				}
			}

		}

		/// <summary>
		/// inject all tabs path to sub sections
		/// </summary>
		void injectSubSections(WrapperTabs state)
		{
			// no tabs injected
			if (state == null) return;

			for (int i = 0; i < state.countTabs; i++)
			{
				var t = state.getTabByIndex(i);

				if (t is TabSceneSelector tss)
				{
					injectSubSection(tss.Path);
				}
				else Debug.LogWarning($"injection issue ? injected tab {t} is not a <TabSceneSelector>");
			}
		}

		void injectSubSection(string sectionPath)
		{
			//if (verbose) Debug.Log("SceneSelector :: refresh section : " + sectionPath);
			if (string.IsNullOrEmpty(sectionPath))
				return;

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

			//Debug.Log("sorting x" + profils.Count + " profiles");

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

			//Debug.Log("solved x" + output.Count + " subs");

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

			log("category <b>" + category + "</b>   match paths x" + cat_paths.Count);

			// filter singles
			List<string> singles = new List<string>();
			for (int i = 0; i < cat_paths.Count; i++)
			{
				string p = cat_paths[i];
				string context = SceneProfil.extractContextFromPath(SceneTools.removePathBeforeFile(p));

				if (!singles.Contains(context)) singles.Add(context);
				else
				{
					cat_paths.RemoveAt(i);
					i--;
				}
			}

			log("singles category?<b>" + category + "</b>   remaining paths x" + cat_paths.Count);

			for (int i = 0; i < cat_paths.Count; i++)
			{
				string path = cat_paths[i];

#if UNITY_EDITOR
				if (useProgressBar())
				{
					float progr = (i * 1f) / (cat_paths.Count * 1f);
					if (UnityEditor.EditorUtility.DisplayCancelableProgressBar("profil : " + category, "..." + path, progr))
					{
						return null;
					}
				}
#endif

				// generate a profil with given path
				var sp = generateProfil(path);

				// check if the profil is already part of profils[]
				if (!sp.HasLayers)
				{
					Debug.LogWarning(path + " has no content");
					continue;
				}

				profils.Add(sp);
				log(" ADDED PROFIL  label:" + sp.label + " (lyrx" + sp.layers.Count + ") @ " + path);
			}

			log("solved x" + profils.Count + " profiles");
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