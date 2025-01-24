using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace fwp.scenes.examples
{
    using fwp.utils.editor.tabs;

    public class WinEdTabsExample : WinEdTabs
    {
        [MenuItem("Screen/tabs", false, 1)]
        static public void init() => GetWindow(typeof(WinEdTabsExample));

        public override void populateTabsEditor(WrapperTabs wt)
        {
            wt.addTab(this, "default", tabDefault);
            wt.addTab(this, "alt", tabAlt);
        }

        public override void populateTabsRuntime(WrapperTabs wt)
        {
            base.populateTabsRuntime(wt);

            wt.addTab(this, "alt runtime", tabRunAlt);
            wt.addTab(this, "rnd runtime", tabRandom);
        }

        void tabRandom()
        {
            GUILayout.Label("stuff");
        }

        void tabRunAlt()
        {
            GUILayout.Label("drawing default tab");
        }

        void tabDefault()
        {
            GUILayout.Label("drawing default tab");
        }

        void tabAlt()
        {
            GUILayout.Label("drawing default alt");
        }

        protected override string getWindowTabName()
        {
            var state = tabsState;
            if (state != null)
            {
                return "tab " + tabsState.getWrapperUid() + "#" + tabsState.tabActive;
            }
            else
            {
                return "tab : no state";
            }

        }
    }
}