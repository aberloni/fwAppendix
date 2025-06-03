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
        //[MenuItem("Screen/tabs", false, 1)]
        static public void init() => GetWindow(typeof(WinEdTabsExample));

        public override void populateTabsEditor(WrapperTabs wt)
        {
            wt.addGenericTab("default", tabDefault);
            wt.addGenericTab("alt", tabAlt);
        }

        public override void populateTabsRuntime(WrapperTabs wt)
        {
            base.populateTabsRuntime(wt);

            wt.addGenericTab("alt runtime", tabRunAlt);
            wt.addGenericTab("rnd runtime", tabRandom);
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