using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.screens
{
    /// <summary>
    /// meant to be a menu/scene that will pop in front of gameplay
    /// </summary>
    public class OverlayAnimated : ScreenAnimated
    {
        
        protected override void validate()
        {
            base.validate();

            if (type != ScreenType.overlay)
                type = ScreenType.overlay;
        }

        /// <summary>
        /// not really useful
        /// </summary>
        static public void closeOverlay(OverlayAnimated overlay)
        {
            overlay.closeAnimated();
        }

        const string overlay_prefix = "overlay-";

        /// <summary>
        /// auto add pefix
        /// </summary>
        static public void openOverlay(System.Enum value, System.Action<OverlayAnimated> onCompletion = null)
            => openOverlay(value.ToString(), onCompletion);

        /// <summary>
        /// auto add pefix
        /// </summary>
        static public void openOverlay(string overlayName, System.Action<OverlayAnimated> onCompletion = null)
        {
            if (!overlayName.StartsWith(overlay_prefix)) overlayName = overlay_prefix + overlayName;

            // if already open callback is done in same frame
            // and show() is called automatically
            ScreensManager.open(overlayName.ToString(), (screen) =>
            {
                OverlayAnimated ao = screen as OverlayAnimated;
                Debug.Assert(ao != null, screen + "  is not castable to overlay");
                onCompletion?.Invoke(ao);
            });
        }

    }

}
