using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using fwp.screens;

public class OverlayTestC : ScreenOverlay
{

    protected override void screenSetup()
    {
        base.screenSetup();

        subSkip(() =>
        {
            Debug.Log("skip");
        });
    }

    protected override bool isOpenDuringSetup()
    {
        return false;
    }

}
