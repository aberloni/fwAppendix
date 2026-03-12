using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    protected override void updateScreenVisible()
    {
        base.updateScreenVisible();

        //Debug.Log("visible");
    }

    
}
