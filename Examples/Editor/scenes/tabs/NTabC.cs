using UnityEngine;

namespace fwp.examples
{
    public class NTabC : fwp.utils.editor.tabs.WrapperTab
    {
        public NTabC() : base("TabC")
        {
        }

        public override bool IsAvailable()
        {
            return NTabA.tabC;
        }

        protected override void drawGUI()
        {
            base.drawGUI();

            GUILayout.Label("tab : " + GetTabLabel());
        }
    }
}