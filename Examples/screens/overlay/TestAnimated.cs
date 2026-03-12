using System.Collections;
using System.Collections.Generic;
using SwitchBuildUtils;
using UnityEngine;

public class TestAnimated : fwp.screens.ScreenAnimated
{
	protected override bool isOpenDuringSetup() => true;

	const string bool_fade = "fade";
	const string state_fadein = "overlay_fadein";
	const string state_fadeout = "overlay_fadeout";

    protected override Parameters generateAnimatedParams()
    {
        base.generateAnimatedParams();
		parameters.state_closed = "overlay_closed";
		parameters.state_opened = "overlay_opened";
		return parameters;
    }

	protected override void onOpeningEnded()
	{
		base.onOpeningEnded();
		Animator.SetBool(bool_fade, true);
	}
	
	protected override void updateScreenVisible()
	{
		base.updateScreenVisible();

		var state = Animator.GetCurrentAnimatorStateInfo(0);

		if (state.IsName(state_fadein))
		{
			Animator.SetBool(bool_fade, false);
		}

		if (state.IsName(state_fadeout))
		{
			close();
		}
	}
}
