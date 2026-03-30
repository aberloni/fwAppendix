using UnityEngine;

namespace fwp.examples
{

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

		protected override void reactAfterOpening()
		{
			base.reactAfterOpening();

			Animator.SetBool(bool_fade, true);
		}

		public override void menuUpdate()
		{
			base.menuUpdate();
			// Debug.Log("visible?" + isVisible());Debug.Log("interact?" + isInteractable());
		}

		protected override void updateInteractable()
		{
			base.updateInteractable();

			var state = Animator.GetCurrentAnimatorStateInfo(0);
			Debug.Log(state.shortNameHash);

			if (state.IsName(state_fadein))
			{
				Debug.Log("fade.out");
				Animator.SetBool(bool_fade, false);
			}

			if (state.IsName(state_fadeout))
			{
				Debug.Log("close");
				close();
			}
		}
	}

}