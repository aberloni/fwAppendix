namespace fwp.utils.editor
{
	/// <summary>
	/// PROVIDE:
	/// refresh, update, draw
	/// 
	/// meant to provide a way to force refresh of a window
	/// and encapsulate the refresh content event
	/// </summary>
	abstract public class WinEdRefreshable : WinEdFilterable
	{
		bool _refresh = false;

		protected bool optinFocusRefresh = false;

        protected override void build()
        {
            base.build();
			refresh();
        }

		protected override void onFocus(bool gainFocus)
		{
			base.onFocus(gainFocus);

			if (optinFocusRefresh && gainFocus)
			{
				refresh(_refresh); // passive : on focus gain
			}
		}

		/// <summary>
		/// ask for a refresh
		/// (ie during GUI phase)
		/// </summary>
		public void primeRefresh()
		{
			_refresh = true;
			log("refresh <b>primed</b>");
		}

		protected override void onRefreshClicked()
		{
			base.onRefreshClicked();
			UnityEngine.Profiling.Profiler.BeginSample("fwp.refresh");
			refresh(true);
			UnityEngine.Profiling.Profiler.EndSample();
		}

		/// <summary>
		/// called onFocus gained, force=false
		/// </summary>
		virtual public void refresh(bool force = false)
		{
			_refresh = false;
			if (force) logw("<b>refresh</b> forced");
		}

	}
}
