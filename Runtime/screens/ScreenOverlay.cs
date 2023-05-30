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

        protected override void screenCreated()
        {
            base.screenCreated();

            Debug.Assert(type == ScreenObject.ScreenType.overlay);
        }

        protected override void onClosingAnimationCompleted()
        {
            //base.onClosingAnimationCompleted();

            //Debug.Log(tags);

            if(tags.HasFlag(ScreenTags.stickyPersistance))
            {
                // just hide
                hide();
            }
            else
            {
                // remove screen
                unload();
            }
        }

        const string overlayPrefix = "overlay-";

        static string filterName(string name)
        {
            if(!name.StartsWith(overlayPrefix))
            {
                name = overlayPrefix + name;
            }
            return name;
        }

        /// <summary>
        /// will autocomplete prefix "overlay-"
        /// </summary>
        static public void openOverlay(string overlayName, Action<ScreenObject> onLoaded = null)
        {
            ScreensManager.open(filterName(overlayName), onLoaded);
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
