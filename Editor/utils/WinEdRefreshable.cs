using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

		protected override void onFocus(bool gainFocus)
		{
			base.onFocus(gainFocus);

			if (gainFocus)
			{
				refresh(); // passive : on focus gain
			}
		}

		override protected void update()
		{
			if (_refresh)
			{
				_refresh = false;
				UnityEngine.Profiling.Profiler.BeginSample("fwp.refresh");
				refresh(true); // primed active refresh
				UnityEngine.Profiling.Profiler.EndSample();
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
			primeRefresh();
		}

		/// <summary>
		/// called onFocus gained, force=false
		/// </summary>
		virtual public void refresh(bool force = false)
		{
			if (force) log("<b>refresh</b> forced");
		}

	}
}
