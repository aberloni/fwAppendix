using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.utils.editor.tabs
{
	/// <summary>
	/// PROVIDE:
	/// tabs for refreshable window
	/// 
	/// usage:
	/// override generate methods to feed your own content
	/// </summary>
	abstract public class WinEdTabs : WinEdRefreshable
	{
		/// <summary>
		/// all tabs used while out of runtime
		/// </summary>
		WrapperTabs stateEditime;

		/// <summary>
		/// all tabs used during runtime
		/// </summary>
		WrapperTabs stateRuntime;

		/// <summary>
		/// tabs @editime or @runtime
		/// </summary>
		protected WrapperTabs ActiveTabs => Application.isPlaying ? stateRuntime : stateEditime;

		/// <summary>
		/// what tabs to draw !runtime
		/// tab label, gui draw callback
		/// return : true to draw additionnal content
		/// </summary>
		abstract public void populateTabsEditor(WrapperTabs tabsSilo);

		/// <summary>
		/// what to draw @runtime
		/// default is same as editor
		/// </summary>
		virtual public void populateTabsRuntime(WrapperTabs tabsSilo)
		{
			populateTabsEditor(tabsSilo); // default is runtime = editor
		}

		public void resetTabSelection()
		{
			selectTab(0);
		}

		public void selectTab(int index)
		{
			if (ActiveTabs.tabActiveIndex != index)
			{
				ActiveTabs.tabActiveIndex = index;
				onTabIndexChanged();
			}
		}

		virtual protected void onTabIndexChanged() { }

		override public void refresh(bool force = false)
		{
			base.refresh(force);

			if (force || stateEditime == null || !stateEditime.isSetup)
			{
				stateEditime = new WrapperTabs("editor-" + GetType());
				populateTabsEditor(stateEditime);

				stateRuntime = new WrapperTabs("runtime-" + GetType());
				populateTabsRuntime(stateRuntime);
			}
		}

		sealed protected override void draw()
		{
			base.draw();

			var _state = ActiveTabs;

			if (_state == null)
			{
				GUILayout.Label("no tabs available");
				return;
			}

			drawFilterField();

			// above tabs buttons
			drawAboveTabsHeader();

			// tabs buttons
			// +oob check
			// & draw active tab
			_state.Draw();
		}

		/// <summary>
		/// some space above tabs buttons
		/// </summary>
		virtual protected void drawAboveTabsHeader()
		{ }

	}

}
