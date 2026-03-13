using UnityEngine;

namespace fwp.examples
{

    using fwp.screens;

    /// <summary>
    /// test: open and close overlays
    /// </summary>
    public class OverlayTests : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                toggle("test-b");
            }

            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                toggle("test-a");
            }

            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                ScreensManager.load("overlay-test-c");
            }

            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                toggle("test-c");
            }
        }

        void toggle(string nm)
        {

            var overlay = ScreenOverlay.getOverlay(nm);
            if (overlay != null)
            {
                //overlay.verbose = true;

                if (overlay.isVisible())
                {
                    overlay.close();
                }
                else
                {
                    overlay.open();
                }


            }
            else
            {
                ScreenOverlay.openOverlay(nm);
            }
        }

    }

}