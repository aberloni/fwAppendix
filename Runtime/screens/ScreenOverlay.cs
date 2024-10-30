using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace fwp.screens
{

    /// <summary>
    /// meant to be something that prompt in front of game context
    /// and be remove (if not persistant)
    /// </summary>
    public class ScreenOverlay : ScreenAnimated
    {
        const string overlayPrefix = "overlay-";

        protected override void screenCreated()
        {
            base.screenCreated();

            Debug.Assert(type == ScreenObject.ScreenType.overlay, "INTE, screen must be overlay type");
        }

        /// <summary>
        /// add "overlay-" in front is missing
        /// </summary>
        static public string filterName(string name)
        {
            if(!name.StartsWith(overlayPrefix))
            {
                name = overlayPrefix + name;
            }
            return name;
        }

        static public ScreenWrapper openOverlayDelayed(string overlayName, Action<ScreenObject> onOpened = null, Action onEnded = null)
        {
            var ret = ScreenWrapper.call(filterName(overlayName));
            ret.setupCallbacks(null, onOpened, onEnded);
            return ret;
        }

        /// <summary>
        /// will autocomplete prefix "overlay-"
        /// </summary>
        static public void openOverlay(string overlayName, Action<ScreenObject> onLoaded = null)
        {
            ScreensManager.open(filterName(overlayName), onLoaded);
        }

        static public void closeOverlay(string overlayName)
        {
            ScreensManager.close(filterName(overlayName));
        }

        static public ScreenWatcher watchOverlay(ScreenOverlay overlay,
            Action onCreated = null, Action onOpened = null, Action onClosed = null)
        {
            return ScreenWatcher.create(filterName(overlay.name),
                onCreated, onOpened, onClosed);
        }

        static public ScreenOverlay getOverlay(string overlayName)
        {
            return getScreen<ScreenOverlay>(filterName(overlayName));
        }
    }

}
