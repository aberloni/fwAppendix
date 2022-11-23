using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace fwp.scenes
{
	abstract public class WinEdBlueprintScenesProfiler : EditorWindow
	{

		string[] sections;
		Dictionary<string, List<SceneProfil>> buttons;

		int tabActive = 0;
		GUIContent[] tabs;
		Vector2 tabScroll;

		abstract protected string[] generateSections();

		private void Update()
		{
			if (sections == null)
			{
				refreshLists(true);
			}
		}

		void OnGUI()
		{
			if (GUILayout.Button("refresh refs"))
			{
				refreshLists(true);
			}

			if (sections == null) return;
			if (buttons == null) return;

			//HalperPrefsEditor.drawToggle("upfold hierarchy", HalperPrefsEditor.ppref_editor_lock_upfold);

			GUILayout.Space(10f);

			tabActive = generateTabsHeader(tabActive, tabs);

			string nm = sections[tabActive];
			var section = buttons[nm];

			GUILayout.Label($"{nm} has x{section.Count} available scenes");

			tabScroll = GUILayout.BeginScrollView(tabScroll);

			for (int i = 0; i < section.Count; i++)
			{
				if (GUILayout.Button(section[i].uid)) // each profil
				{
					//if (EditorPrefs.GetBool(edLoadDebug)) section[i].loadDebug = true;
					section[i].editorLoad();
				}
			}
			GUILayout.EndScrollView();
		}

		void refreshLists(bool force = false)
		{
			if (buttons == null || force)
			{
				buttons = new Dictionary<string, List<SceneProfil>>();

				sections = generateSections();

				for (int i = 0; i < sections.Length; i++)
				{
					var list = getProfils(sections[i]);
					buttons.Add(sections[i], list);
				}
			}


			if (tabs == null)
			{
				tabs = generateTabsDatas(sections);
			}
		}


		List<SceneProfil> getProfils(string cat)
		{
			List<string> names = SceneTools.getScenesNamesOfCategory(cat);

			//Debug.Log("category:" + cat+" has x"+names.Count);

			List<SceneProfil> profils = new List<SceneProfil>();
			for (int i = 0; i < names.Count; i++)
			{
				SceneProfil sp = new SceneProfil(names[i]);

				//sp.loadDebug = EditorPrefs.GetBool(edLoadDebug);

				sp.reload();

				profils.Add(sp);
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
