using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace fwp.scenes
{
	abstract public class WinEdBlueprintScenesProfiler : EditorWindow
	{
		List<string> paths = new List<string>();

		string[] sections;
		Dictionary<string, List<SceneProfil>> buttons;

		int tabActive = 0;
		GUIContent[] tabs;
		Vector2 tabScroll;

		abstract protected string[] generateSections();

		private void Update()
		{
			if (sections == null || buttons == null)
			{
				refreshLists(true);
			}
		}

		/// <summary>
		/// use draw() to add content
		/// </summary>
		void OnGUI()
		{
			if (GUILayout.Button("Scenes selector", halpers.HalperGuiStyle.getWinTitle()))
			{
				refreshLists(true);
			}

			if (sections == null) return;
			if (buttons == null) return;

			//HalperPrefsEditor.drawToggle("upfold hierarchy", HalperPrefsEditor.ppref_editor_lock_upfold);

			GUILayout.Label($"selector found a total of x{paths.Count} scenes in project");

			GUILayout.Space(10f);

			tabActive = generateTabsHeader(tabActive, tabs);

			string nm = sections[tabActive];
			var sectionContent = buttons[nm];

			GUILayout.BeginHorizontal();
			GUILayout.Label($"{nm} has x{sectionContent.Count} available scenes");
			if(GUILayout.Button("ping folder"))
            {
				fwp.halpers.editor.HalperEditor.pingFolder(nm);
            }
			GUILayout.EndHorizontal();

			GUILayout.Space(10f);

			tabScroll = GUILayout.BeginScrollView(tabScroll);

			for (int i = 0; i < sectionContent.Count; i++)
			{
				if (GUILayout.Button(sectionContent[i].uid)) // each profil
				{
					//if (EditorPrefs.GetBool(edLoadDebug)) section[i].loadDebug = true;
					sectionContent[i].editorLoad();
				}
			}

			GUILayout.EndScrollView();

			draw();
		}

		/// <summary>
		/// additionnal stuff within scrollview
		/// </summary>
		virtual protected void draw()
        {

        }

		protected void refreshLists(bool force = false)
		{
			if (buttons == null || force)
			{
				buttons = new Dictionary<string, List<SceneProfil>>();

				sections = generateSections();

				paths.Clear();

				for (int i = 0; i < sections.Length; i++)
				{
					var list = getProfils(sections[i]);
					buttons.Add(sections[i], list);
				}
			}


			if (tabs == null || force)
			{
				tabs = generateTabsDatas(sections);
			}
		}


		protected List<SceneProfil> getProfils(string cat)
		{
			var cat_paths = SceneTools.getScenesNamesOfCategory(cat).ToArray();

			//Debug.Log("category:" + cat+" has x"+names.Count);

			List<SceneProfil> profils = new List<SceneProfil>();
			for (int i = 0; i < cat_paths.Length; i++)
			{
				SceneProfil sp = new SceneProfil(cat_paths[i]);

				//sp.loadDebug = EditorPrefs.GetBool(edLoadDebug);

				sp.reload();

				profils.Add(sp);

				paths.Add(cat_paths[i]);
			}

			return profils;
		}

		static public GUIContent[] generateTabsDatas(string[] labels)
		{
			GUIContent[] modeLabels = new GUIContent[labels.Length];

			//GUILayout.Label("no mode labels (x" + nms.Length + " candidates)");

			for (int i = 0; i < labels.Length; i++)
			{
				modeLabels[i] = new GUIContent(labels[i], "tooltip");
			}

			return modeLabels;
		}

		static public int generateTabsHeader(int tabSelected, GUIContent[] tabs)
		{
			//GUIStyle gs = new GUIStyle(GUI.skin.button)
			//int newTab = GUILayout.Toolbar((int)tabSelected, modeLabels, "LargeButton", GUILayout.Width(toolbarWidth), GUILayout.ExpandWidth(true));
			int newTab = GUILayout.Toolbar((int)tabSelected, tabs, "LargeButton");
			//if (newTab != (int)tabSelected) Debug.Log("changed tab ? " + tabSelected);

			return newTab;
		}

	}

}
