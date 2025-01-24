using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using fwp.utils.editor.tabs;

using fwp.scenes;
using fwp.scenes.editor;

namespace fwp.scenes.examples
{

    public class WinEdSceneSelector : WinEdBlueprintScenesSelector
    {
        [UnityEditor.MenuItem("Screen/scenes", false, 1)]
        static public void init() => GetWindow(typeof(WinEdSceneSelector));

        public override void populateTabsEditor(WrapperTabs wt)
        {
            wt.addSpecific(new TabSceneSelector(this, "scenes/Regions/Biomes"));
            wt.addSpecific(new TabSceneSelector(this, "scenes/LDs"));
        }

        protected override SceneProfil generateProfil(string uid)
        {
            //return base.generateProfil(uid);
            return new CustomProfil(uid);
        }

        protected override SceneSubFolder generateSub(string profilUid)
        {
            //return base.generateSub(profilUid);
            return new CustomSceneSub(profilUid);
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
            var state = tabsState;
            if (state != null)
            {
                return "scene selector " + tabsState.getWrapperUid() + "#" + tabsState.tabActive;
            }
            else
            {
                return "scene selector : no state";
            }

        }
    }
}