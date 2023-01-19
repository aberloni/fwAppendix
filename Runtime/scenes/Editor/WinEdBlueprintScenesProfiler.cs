using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

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
				Debug.Log("force refresh");
				refreshLists(true);
			}

			if (sections == null) return;
			if (buttons == null) return;

			GUILayout.Label($"selector found a total of x{paths.Count} scenes in project");

			GUILayout.Space(10f);

			tabActive = generateTabsHeader(tabActive, tabs);

			string nm = sections[tabActive];
			var profils = buttons[nm];

			GUILayout.BeginHorizontal();
			GUILayout.Label($"{nm} has x{profils.Count} available scenes");
			if(GUILayout.Button("ping folder"))
            {
				pingFolder(nm);
            }
			GUILayout.EndHorizontal();

			GUILayout.Space(10f);

			tabScroll = GUILayout.BeginScrollView(tabScroll);

			//var openedProfil = getOpenedProfil();

			foreach(var profil in profils)
			{
				GUILayout.BeginHorizontal();

				// scene button
				if (GUILayout.Button(profil.editor_getButtonName())) // each profil
				{
					//if (EditorPrefs.GetBool(edLoadDebug)) section[i].loadDebug = true;
					profil.editorLoad(false);
				}

				// add/remove buttons
				bool present = appendix.AppendixUtils.isSceneLoaded(profil.uid);
				string label = present ? "-" : "+";

				if (GUILayout.Button(label, GUILayout.Width(40f)))
                {
					if (!present) profil.editorLoad(true);
					else profil.editorUnload();
                }

				GUILayout.EndHorizontal();

				/*
				if(openedProfil == profil)
                {
					EditorGUILayout.Space(10f);

                    for (int i = 0; i < profil.layers.Count; i++)
                    {
						drawButtonSceneSteam(profil.layers[i], profil.layers[i]);
					}

					EditorGUILayout.Space(10f);
				}
				*/

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

		void drawButtonSceneSteam(string label, string path)
        {

			GUILayout.BeginHorizontal(GUILayout.Width(200f));

			// scene button
			if (GUILayout.Button(label)) // each profil
			{
				SceneLoaderEditor.loadScene(path);
			}

			// add/remove buttons
			bool present = SceneTools.isSceneLoaded(path);
			label = present ? "-" : "+";

			if (GUILayout.Button(label, GUILayout.Width(40f)))
			{
				if (!present) SceneLoaderEditor.loadScene(path);
				else
				{
					SceneLoaderEditor.unloadScene(path);
				}
			}

			GUILayout.EndHorizontal();

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
			List<SceneProfil> profils = new List<SceneProfil>();

			// works with Contains
			var cat_paths = SceneTools.getScenesPathsOfCategory(cat);

            Debug.Log("category:" + cat + " paths x" + cat_paths.Count);

            foreach (string path in cat_paths)
            {
				SceneProfil sp = generateProfil(path);
				if(sp.isValid())
                {
					bool found = false;

					foreach(var profil in profils)
                    {
						if (profil.match(sp))
							found = true;
                    }

					if(!found)
						profils.Add(sp);
				}
			}

			return profils;
		}

		public SceneProfil getOpenedProfil()
        {
			var category = buttons[sections[tabActive]];
			foreach(var profil in category)
			{
				if (profil.isLoaded()) return profil;
            }
			return null;
        }

		abstract protected SceneProfil generateProfil(string uid);
        
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
