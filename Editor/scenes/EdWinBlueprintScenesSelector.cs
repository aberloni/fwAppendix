using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace fwp.scenes
{
    using fwp.appendix;

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
    abstract public class EdWinBlueprintScenesSelector : fwp.utils.editor.EdWinTabs
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
        virtual protected SceneSubFolder generateSub(string profilUid)
        {
            return new SceneSubFolder(profilUid);
        }

        protected override void refreshByTitle()
        {
            SceneProfil.verbose = true;
            base.refreshByTitle();
            SceneProfil.verbose = false;
        }

        protected override void refresh(bool force = false)
        {
            base.refresh(force);

            var state = tabsState; // getter edit/runtime tabs

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
                var lbl = state.tabs[i].path;
                
                if (verbose) Debug.Log("SceneSelector :: refresh section : " + lbl);

                List <SceneSubFolder> tabContent = solveTabFolder(lbl);
                sections.Add(lbl, tabContent);
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

            if (GUILayout.Button("ping folder", GUILayout.Width(GuiHelpers.btnLabelWidth)))
            {
                pingFolder(Path.Combine(tabLabel));
            }

            if (GUILayout.Button("upfold all", GUILayout.Width(GuiHelpers.btnLabelWidth)))
            {
                for (int i = 0; i < subList.Count; i++)
                {
                    subList[i].toggled = false;
                }
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

            // all profil will be matched based on the parent path
            foreach (SceneProfil profil in profils)
            {
                string parent = profil.parentPath;

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

                if(verbose) Debug.Log(sub.stringify());

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
                Debug.Log("category:" + category + " match paths x" + cat_paths.Count);



            for (int i = 0; i < cat_paths.Count; i++)
            {
                string path = cat_paths[i];

#if UNITY_EDITOR
                float progr = (i * 1f) / (cat_paths.Count * 1f);
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar("profil : " + category, "..."+path, progr))
                {
                    return null;
                }
#endif

                SceneProfil sp = generateProfil(path);
                if (sp.isValid())
                {
                    bool found = false;

                    //if (verbose) Debug.Log("searching ... " + sp.uid);

                    foreach (var profil in profils)
                    {
                        if (profil.match(sp))
                            found = true;
                    }

                    if (!found)
                    {
                        profils.Add(sp);

                        if (verbose) Debug.Log("+ " + sp.uid);
                    }

                }
            }

            if (verbose)
                Debug.Log("solved x" + profils.Count + " profiles");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif

            return profils;
        }

        public SceneProfil getOpenedProfil()
        {
            var category = sections[tabsState.tabs[tabsState.tabActive].path];

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