using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.screens
{
    /// <summary>
    /// meant to be a menu/scene that will pop during game
    /// and cover the whole screen
    /// interrupt whatever is happening behind
    /// pause gameplay
    /// </summary>
    public class MenuAnimated : ScreenAnimated
    {

        protected override void validate()
        {
            base.validate();

            if (type != ScreenType.menu)
                type = ScreenType.menu;
        }
    }

}
