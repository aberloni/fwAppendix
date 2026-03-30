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

			Debug.Log("open.after");
		}

        protected override void reactBeforeOpening()
        {
            base.reactBeforeOpening();
			Debug.Log("open.before");
        }

        protected override void reactBeforeClosing()
        {
            base.reactBeforeClosing();
			Debug.Log("close.before");
        }

        protected override void reactAfterClosing()
        {
            base.reactAfterClosing();
			Debug.Log("close.after");
        }

        protected override void updateInteractable()
        {
            base.updateInteractable();
			
			var state = Animator.GetCurrentAnimatorStateInfo(0);

			if (state.IsName(state_fadein))
			{
				Animator.SetBool(bool_fade, false);
			}

			if (state.IsName(state_fadeout))
			{
				Debug.Log("CLOSE");
				close();
			}
		}
	}

}