using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace fwp.scenes
{
    using fwp.utils.editor;
	using fwp.settings.editor;

	/// <summary>
	/// gather all scenes profiles for a specific folder
	/// scenes[] will be override externaly
	/// permet de regrouper les sceneprofil dans un même container
	/// </summary>
	public class SceneSubFolder
    {
        public string projectPath; // where the folder is located in Assets/
        public string folderName; // just folder name

        public string completePath => System.IO.Path.Combine(projectPath, folderName);

        public List<SceneProfil> profils = null;

        public bool toggled
        {
            set
            {
                EditorPrefs.SetBool(completePath, value);
            }
            get
            {
                return EditorPrefs.GetBool(completePath, false);
            }
        }

        public SceneSubFolder(string folderPath)
        {
            projectPath = folderPath;

            if (projectPath.Length <= 0)
            {
                Debug.LogWarning("no base path given ?");
            }

            folderName = folderPath.Substring(folderPath.LastIndexOf("/") + 1);
        }

        public bool hasContentMatchingFilter(string filter)
        {
            if (string.IsNullOrEmpty(filter)) return profils.Count > 0;

            int cnt = 0;
            for (int i = 0; i < profils.Count; i++)
            {
                //Debug.Log(scenes[i].label + " vs " + filter);
                if (profils[i].matchFilter(filter))
                    cnt++;
            }

            return cnt > 0;
        }

        public void drawSection(string filter)
        {
            // has any profil matching filter
            if (!hasContentMatchingFilter(filter)) return;

            // sub folder
            toggled = EditorGUILayout.Foldout(toggled, folderName + " (x" + profils.Count + ")", true);
            if (toggled)
            {
                GUILayout.BeginHorizontal();

                if (filter.Length <= 0)
                {
                    GUILayout.Label(" ");
                    if (GUILayout.Button("+all", GUILayout.Width(GuiHelpers.btnSymbLarge)))
                    {
                        if (EditorUtility.DisplayDialog("add all ?", "are you sure ?", "ok", "nope"))
                        {
                            sectionLoadAll();
                        }
                    }
                }

                GUILayout.EndHorizontal();

                foreach (var profil in profils)
                {
                    if (profil.matchFilter(filter))
                    {
                        drawLineContent(profil);
                    }
                }
            }

        }

        void sectionLoadAll()
        {
            Debug.Log("load all");
            foreach (var p in profils)
            {
                p.editorLoad(
                    replaceContext: false,
                    forceAddBuildSettings: true);
            }
        }

        virtual protected void logSceneDetails(SceneProfil profil)
        {
            Debug.Log("profil:" + profil.label);

            Debug.Log("  -> layers x" + profil.layers.Count);
            foreach (var elmt in profil.layers)
                Debug.Log(elmt);

            Debug.Log("  -> deps x" + profil.deps.Count);
            foreach (var dep in profil.deps)
                Debug.Log(dep);

            // and ping scene
            GuiHelpers.pingScene(profil.PingScenePath);
        }

        /// <summary>
        /// whatever is drawn in a profil line
        /// true : pressed button & load is called
        /// </summary>
        virtual protected bool drawLineContent(SceneProfil profil)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("?", GUILayout.Width(GuiHelpers.btnSymbWidthSmall)))
            {
                logSceneDetails(profil);
            }

            bool load = false;

            // scene button
            if (GUILayout.Button(profil.editor_getButtonName())) // each profil
            {
                //if (EditorPrefs.GetBool(edLoadDebug)) section[i].loadDebug = true;
                //profil.editorLoad(false);
                onEditorSceneCall(profil, true);
                load = true;
            }

            // add/remove buttons
            bool present = SceneTools.isEditorSceneLoaded(profil.label);
            //bool present = profil.isLoaded();
            string label = present ? "-" : "+";

            if (GUILayout.Button(label, GUILayout.Width(GuiHelpers.btnSymbWidth)))
            {
                if (!present)
                {
                    onEditorSceneCall(profil, false);
                    reactSceneCall(profil, true);
                    load = true;
                }
                else
                {
                    onEditorSceneRemoval(profil);
                    reactSceneCall(profil, false);
                }
            }

            GUILayout.EndHorizontal();

            return load;
        }

        /// <summary>
        /// when user calls for a scene
        /// load or unload
        /// </summary>
        virtual protected void reactSceneCall(SceneProfil profil, bool load)
        { }

        virtual public string stringify()
        {
            //return "@path:" + projectPath + " @folder:" + folderName + ", total scenes x" + scenes.Count;
            return "@folder:" + folderName + ", total scenes x" + profils.Count;
        }

        public const string _pref_autoAddBuildSettings = "scenesAutoAddBuildSettings";

        /// <summary>
        /// additive only for loading
        /// </summary>
        void onEditorSceneCall(SceneProfil profil, bool replaceContext)
        {
            profil.setDirty();
            profil.editorLoad(replaceContext, MgrEdUserSettings.getBool(_pref_autoAddBuildSettings));
        }

        void onEditorSceneRemoval(SceneProfil profil)
        {
            profil.setDirty();
            profil.editorUnload();
        }

    }

}