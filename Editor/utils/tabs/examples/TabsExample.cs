using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.utils.editor.tabs.sample
{

	public class TabsExample : fwp.utils.editor.tabs.WrapperTabs
	{
		public TabsExample(string wuid) : base(wuid)
		{
			addSpecificTab(new TabExample(wuid + "-A"));
			addSpecificTab(new TabExample(wuid + "-B"));
		}
	}

}