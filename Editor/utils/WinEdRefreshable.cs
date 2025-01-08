using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace fwp.utils.editor
{
    /// <summary>
    /// PROVIDE:
    /// refresh, update, draw
    /// 
    /// meant to provide a way to force refresh of a window
    /// and encapsulate the refresh content event
    /// </summary>
    abstract public class WinEdRefreshable : WinEdFilterable
    {
        bool _refresh = false;

        protected override void onFocus(bool gainFocus)
        {
            base.onFocus(gainFocus);

            if (gainFocus)
            {
                refresh();
            }
        }

        override protected void update()
        {
            if (_refresh)
            {
                _refresh = false;
                refresh(true);
            }
        }

        /// <summary>
        /// ask for a refresh
        /// (ie during GUI phase)
        /// </summary>
        public void primeRefresh()
        {
            _refresh = true;
            log("refresh <b>primed</b>");
        }

        public void refreshVerbose()
        {
            refresh(true);
        }

        protected override void onTitleClicked()
        {
            base.onTitleClicked(); // verbose = true
            refreshVerbose();
        }

        /// <summary>
        /// called onFocus gained, force=false
        /// </summary>
        virtual public void refresh(bool force = false)
        {
            if (force) log("force <b>refresh</b>");
        }

    }
}
