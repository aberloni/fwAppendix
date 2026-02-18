using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace fwp.scenes
{
    using fwp.utils.editor;
    using fwp.settings.editor;
    
    /// <summary>
    /// gather all scenes profils for a specific folder
    /// regroup sceneprofils in a common container
    /// </summary>
    public class SceneSubFolder
    {
        /// <summary>
        /// where the folder is located in Assets/
        /// (without folder name)
        /// </summary>
        public string projectPath;

        /// <summary>
        /// just folder name
        /// </summary>
        public string folderName;

        public string CompletePath => System.IO.Path.Combine(projectPath, folderName);

        protected SceneProfil[] profils;

        public bool IsLoaded
        {
            get
            {
                if (profils == null) return false;
                foreach (var sp in profils)
                {
                    if (sp.isLoaded()) return true;
                }
                return false;
            }
        }

        /// <summary>
        /// editor foldout
        /// </summary>
        public bool Toggled
        {
            set
            {
                EditorPrefs.SetBool(CompletePath, value);
            }
            get
            {
                return EditorPrefs.GetBool(CompletePath, false);
            }
        }

        
        readonly GUIContent gBtnAll = new GUIContent("+all", "load ALL scenes");
        readonly GUIContent gFoldout;

        public SceneSubFolder(string folderPath, SceneProfil[] profils)
        {
            projectPath = folderPath;
            this.profils = profils;

            if (projectPath.Length <= 0)
            {
                Debug.LogWarning("no base path given ?");
            }

            folderName = folderPath.Substring(folderPath.LastIndexOf("/") + 1);

            gFoldout = new GUIContent(folderName + " (x" + this.profils.Length + ")");
        }

        public SceneProfil GetFirstLoadedProfil()
        {
            if (profils == null) return null;
            foreach (var sp in profils)
            {
                if (sp.isLoaded()) return sp;
            }
            return null;
        }

        public bool hasContentMatchingFilter(string filter)
        {
            if (string.IsNullOrEmpty(filter)) return profils.Length > 0;

            int cnt = 0;
            for (int i = 0; i < profils.Length; i++)
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
            bool _toggle = EditorGUILayout.Foldout(Toggled, gFoldout, true);
            if (_toggle != Toggled)
            {
                // Debug.Log("toggled: " + folderName + " = " + _toggle);
                Toggled = _toggle;
            }

            if (_toggle)
            {
                GUILayout.BeginHorizontal();

                if (filter.Length <= 0)
                {
                    GUILayout.FlexibleSpace(); // empty line
                    if (GUILayout.Button(gBtnAll, GUILayout.Width(QuickEditorViewStyles.btnL)))
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
            // Debug.Log("load all");
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
                Debug.Log(elmt.Name);

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

            if (GUILayout.Button(QuickEditorViewStyles.gQuestionMark, GUILayout.Width(QuickEditorViewStyles.btnS)))
            {
                logSceneDetails(profil);
            }

            bool load = false;

            // scene button
            if (GUILayout.Button(profil.label)) // each profil
            {
                //if (EditorPrefs.GetBool(edLoadDebug)) section[i].loadDebug = true;
                //profil.editorLoad(false);
                onEditorSceneCall(profil, true);
                load = true;
            }

            // add/remove buttons
            bool present = SceneTools.isEditorSceneLoaded(profil.Context);

            if (GUILayout.Button(present ? QuickEditorViewStyles.gMinus : QuickEditorViewStyles.gPlus, GUILayout.Width(QuickEditorViewStyles.btnM)))
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
            return "@folder:" + folderName + ", total scenes x" + profils.Length;
        }

        public const string _pref_autoAddBuildSettings = "fwp.scenes.build.settings";

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