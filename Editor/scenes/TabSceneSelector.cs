using UnityEngine;
using System.Collections.Generic;


namespace fwp.scenes.editor
{
	using fwp.utils.editor.tabs;
	using fwp.utils.editor;
	using fwp.scenes;

	/// <summary>
	/// tab with various sections (foldout)
	/// 	containing SceneProfils[]
	/// </summary>
	public class TabSceneSelector : WrapperTab
	{
		string path;
		public string Path => path;

		public string PathEnd => path.Substring(path.LastIndexOf("/") + 1);

		WinEdBlueprintScenesSelector winSelector;

		static readonly GUIContent lblPingFolder = new GUIContent("ping folder");
		static readonly GUIContent lblUpfoldAll = new GUIContent("upfold all");
		static readonly GUIContent lblEmpty = new GUIContent("selector has no sections");

		List<SceneSubFolder> sections = new();

		public TabSceneSelector(WinEdBlueprintScenesSelector window, string path) : base()
		{
			winSelector = window;

			// remove last "/"
			this.path = path.EndsWith("/") ? path.Substring(0, path.LastIndexOf("/") - 1) : path;

			setLabel(PathEnd); // autolabel
		}

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
		virtual protected SceneSubFolder generateSub(string profilUid, SceneProfil[] profils)
		{
			return new SceneSubFolder(profilUid, profils);
		}

		public override void Refresh(bool force)
		{
			base.Refresh(force);

			if (verbose) Debug.Log("refresh forced?" + force);
			
			if (force || sections.Count <= 0)
			{
				if (sections != null)
				{
					foreach (var s in sections) s.Toggled = false;
				}

				sections.Clear();
				sections = solveTabSubFolders(path);
			}

		}

		protected override void drawGUI()
		{
			base.drawGUI();

			if (sections == null || sections.Count <= 0)
			{
				GUILayout.Label(lblEmpty);
				return;
			}

			foreach (var section in sections)
			{
				section.drawSection(winSelector.Filter);
			}

		}

		List<SceneSubFolder> solveTabSubFolders(string tabName)
		{
			List<SceneProfil> profils = edGetProfilsOfCategory(tabName);
			if (profils == null)
			{
				Debug.LogError("null profils while solving tabs ?");
				return null;
			}

			Dictionary<string, List<SceneProfil>> list = new Dictionary<string, List<SceneProfil>>();

			if (verbose) Debug.Log("folder/sorting x" + profils.Count + " profiles");

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
				SceneSubFolder sub = generateSub(kp.Key, kp.Value.ToArray());

				if (verbose) Debug.Log(sub.stringify());

				output.Add(sub);

			}

			if (verbose) Debug.Log("folder/solved x" + output.Count + " subs");

			return output;
		}

		/// <summary>
		/// genere tout les profiles qui sont de la categorie
		/// category is the foldout "folder"
		/// </summary>
		protected List<SceneProfil> edGetProfilsOfCategory(string category)
		{
			List<SceneProfil> profils = new();

			string pbTitle = "getProfils(" + category + ")";
			UnityEditor.EditorUtility.DisplayProgressBar(pbTitle, string.Empty, 0f);

			// fetch all possible path that Contains that category
			var cat_paths = SceneTools.getScenesPathsOfCategory(category, removeExt: true);

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

			if (verbose) Debug.Log("getProfils() category: <b>" + category + "</b> -> total scenes x" + cat_paths.Count + " & total contexts x" + contexts.Count);

			for (int i = 0; i < contexts.Count; i++)
			{
				string ctx = contexts[i];

				UnityEditor.EditorUtility.DisplayProgressBar(pbTitle, "path: " + ctx, (i * 1f) / (contexts.Count * 1f));

				// generate a profil with given path
				var sp = generateProfil(ctx);

				// check if the profil is already part of profils[]
				if (!sp.HasLayers)
				{
					if (verbose) Debug.Log(ctx + " has no content, don't keep this profil");
					continue;
				}

				profils.Add(sp);
				if (verbose) Debug.Log("+PROFIL label:" + sp.label + " (lyr x" + sp.layers.Count + ") @ " + ctx);
			}

			if (verbose) Debug.Log("total profils solved x" + profils.Count);
			// foreach (var p in profils) if(verbose) Debug.Log(p.stringify());

			UnityEditor.EditorUtility.ClearProgressBar();

			return profils;
		}

	}

}