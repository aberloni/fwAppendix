using UnityEngine;

namespace fwp.examples
{
    using fwp.screens;

    public class OverlayTestB : ScreenOverlay
    {

        protected override void screenSetup()
        {
            base.screenSetup();
            subSkip(() =>
            {
                Debug.Log("skip");
            });
        }

        protected override void updateScreenVisible()
        {
            base.updateScreenVisible();

            //Debug.Log("visible");
        }


    }

}
