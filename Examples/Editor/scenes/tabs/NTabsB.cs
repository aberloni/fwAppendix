using UnityEngine;

namespace fwp.examples
{
    public class NTabsB : fwp.utils.editor.tabs.WrapperTabs
    {
        public NTabsB(string containerLabel, string tuid) : base(tuid)
        {
            setContainerLabel(containerLabel);
            addSpecificTab(new NTabA());
            addSpecificTab(new NTabC());
        }
    }
}