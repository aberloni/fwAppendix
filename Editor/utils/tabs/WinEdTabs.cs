using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.utils.editor
{
    using UnityEditor;

    /// <summary>
    /// PROVIDE:
    /// tabs for refreshable window
    /// 
    /// usage:
    /// override generate methods to feed your own content
    /// </summary>
    abstract public class WinEdTabs : WinEdRefreshable
    {

        WrapperTabs stateEditime;
        WrapperTabs stateRuntime;

        protected WrapperTabs tabsState => Application.isPlaying ? stateRuntime : stateEditime;

        /// <summary>
        /// what tabs to draw !runtime
        /// tab label, gui draw callback
        /// return : true to draw additionnal content
        /// </summary>
        abstract public void populateTabsEditor(WrapperTabs wt);

        /// <summary>
        /// what to draw @runtime
        /// default is same as edit time
        /// return null : to draw nothing
        /// func return true : to draw additionnal content
        /// </summary>
        virtual public void populateTabsRuntime(WrapperTabs wt)
        {
            populateTabsEditor(wt);
        }

        public void resetTabSelection()
        {
            tabsState.tabActive = 0;
        }
        public void selectTab(int index)
        {
            tabsState.tabActive = index;
        }

        override public void refresh(bool force = false)
        {
            base.refresh(force);

            if (force || stateEditime == null || !stateEditime.isSetup)
            {
                stateEditime = new WrapperTabs("editor-" + GetType());
                populateTabsEditor(stateEditime);

                stateRuntime = new WrapperTabs("runtime-" + GetType());
                populateTabsRuntime(stateRuntime);
            }

        }

        sealed protected override void draw()
        {
            base.draw();

            var _state = tabsState;

            if (_state == null)
            {
                GUILayout.Label("no tabs available");
                return;
            }

            drawFilterField();

            drawAboveTabsHeader();

            // draw labels buttons
            // +oob check
            if(_state.drawTabsHeader())
            {
                onTabChanged(_state.getActiveTab());
            }

            GUILayout.Space(15f);

            _state.drawActiveTab();
        }

        virtual protected void drawAboveTabsHeader()
        { }

        virtual protected void onTabChanged(WrapperTab tab)
        {
            if (verbose) Debug.Log("selected tab #" + tab);
        }


    }

}
