using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace fwp.scenes
{
	/// <summary>
	/// how to use :
	/// - inherite of this class to have your own window
	/// - implement sections names for tabs
	/// - you can override generateProfil to use some specific SceneProfil
	/// </summary>
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
			if (GUILayout.Button("Scenes selector", getWinTitle()))
			{
				refreshLists(true);
			}

			if (sections == null) return;
			if (buttons == null) return;

			GUILayout.Label($"selector found a total of x{paths.Count} scenes in project");

			GUILayout.Space(10f);

			tabActive = generateTabsHeader(tabActive, tabs);

			string nm = sections[tabActive];
			var sectionContent = buttons[nm];

			GUILayout.BeginHorizontal();
			GUILayout.Label($"{nm} has x{sectionContent.Count} available scenes");
			if(GUILayout.Button("ping folder"))
            {
				pingFolder(nm);
            }
			GUILayout.EndHorizontal();

			GUILayout.Space(10f);

			tabScroll = GUILayout.BeginScrollView(tabScroll);

			for (int i = 0; i < sectionContent.Count; i++)
			{
				GUILayout.BeginHorizontal();

				// scene button
				if (GUILayout.Button(sectionContent[i].editor_getButtonName())) // each profil
				{
					//if (EditorPrefs.GetBool(edLoadDebug)) section[i].loadDebug = true;
					sectionContent[i].editorLoad(false);
				}

				// add/remove buttons
				bool present = appendix.AppendixUtils.isSceneLoaded(sectionContent[i].uid);
				string label = present ? "-" : "+";

				if (GUILayout.Button(label, GUILayout.Width(40f)))
                {
					if (!present) sectionContent[i].editorLoad(true);
					else sectionContent[i].editorUnload();
                }

				GUILayout.EndHorizontal();
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
			// works with Contains
			var cat_paths = SceneTools.getScenesNamesOfCategory(cat).ToArray();

			//Debug.Log("category:" + cat+" has x"+names.Count);
			
			List<string> uids = new List<string>();
            for (int i = 0; i < cat_paths.Length; i++)
            {
				string uid = extractUid(cat_paths[i]);
				if (!uids.Contains(uid)) uids.Add(uid);
			}

			SceneProfil sp;

			List<SceneProfil> profils = new List<SceneProfil>();
			for (int i = 0; i < uids.Count; i++)
			{
				sp = generateProfil(uids[i]);

				profils.Add(sp);

				paths.Add(cat_paths[i]);
			}

			return profils;
		}

		/// <summary>
		/// beeing able to solve uids differently
		/// like : scene-name_layer => scene-name
		/// </summary>
		virtual protected string extractUid(string path)
        {
			// scene-name_layer => scene-name
			if (path.IndexOf('_') > 0)
            {
				return path.Substring(0, path.IndexOf('_'));
            }

			return path;
        }

		virtual protected SceneProfil generateProfil(string uid)
        {
			return new SceneProfil(uid);
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



		static private GUIStyle gWinTitle;
		static public GUIStyle getWinTitle()
		{
			if (gWinTitle == null)
			{
				gWinTitle = new GUIStyle();

				gWinTitle.richText = true;
				gWinTitle.alignment = TextAnchor.MiddleCenter;
				gWinTitle.normal.textColor = Color.white;
				gWinTitle.fontSize = 20;
				gWinTitle.fontStyle = FontStyle.Bold;
				gWinTitle.margin = new RectOffset(10, 10, 10, 10);
				//gWinTitle.padding = new RectOffset(30, 30, 30, 30);

			}

			return gWinTitle;
		}

		/// <summary>
		/// use : EditorGUIUtility.PingObject
		/// </summary>
		static public void pingFolder(string assetsPath)
		{
			string path = "Assets/" + assetsPath;

			// Load object
			UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

			// Select the object in the project folder
			Selection.activeObject = obj;

			// Also flash the folder yellow to highlight it
			EditorGUIUtility.PingObject(obj);
		}

	}

}
