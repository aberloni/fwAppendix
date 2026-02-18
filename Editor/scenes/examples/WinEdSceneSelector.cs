using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using fwp.utils.editor.tabs;

using fwp.scenes.editor;

namespace fwp.scenes.examples
{
    /// <summary>
    /// example
    /// </summary>
    public class WinEdSceneSelector : WinEdBlueprintScenesSelector
    {
        //[UnityEditor.MenuItem("Screen/scenes", false, 1)]
        static public void init() => GetWindow(typeof(WinEdSceneSelector));

        public override void populateTabsEditor(WrapperTabs wt)
        {
            wt.addSpecificTab(new TabCustom(this, "scenes/Regions/Biomes"));
            wt.addSpecificTab(new TabCustom(this, "scenes/LDs"));
        }

        protected override void drawFooter()
        {
            base.drawFooter();

            GUILayout.Label("additionnal");
            
            if (GUILayout.Button("ping halpers/"))
            {
                selectFolder("halpers/", true);
            }
        }

        protected override string getWindowTabName()
        {
            var state = ActiveTabs;
            if (state != null)
            {
                return "scene selector " + ActiveTabs.getWrapperUid() + "#" + ActiveTabs.tabActiveIndex;
            }
            else
            {
                return "scene selector : no state";
            }

        }
    }

    public class TabCustom : TabSceneSelector
    {
        public TabCustom(WinEdBlueprintScenesSelector window, string path) : base(window, path)
        {
        }

        protected override SceneProfil generateProfil(string uid)
        {
            //return base.generateProfil(uid);
            return new CustomProfil(uid);
        }

        protected override SceneSubFolder generateSub(string profilUid, SceneProfil[] profils)
        {
            //return base.generateSub(profilUid);
            return new CustomSceneSub(profilUid, profils);
        }

    }
}