using UnityEngine;

namespace fwp.examples
{

    using fwp.screens;

    public class OverlayTestA : ScreenOverlay
    {

        protected override void screenSetup()
        {
            base.screenSetup();
            subSkip(() =>
            {
                Debug.Log("skip");
            });
        }
        

    }

}