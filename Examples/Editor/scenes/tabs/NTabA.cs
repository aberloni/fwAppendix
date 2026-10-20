using UnityEngine;

namespace fwp.examples
{
    public class NTabA : fwp.utils.editor.tabs.WrapperTab
    {
        static public bool tabC;

        public NTabA() : base("TabA")
        { }

        protected override void drawGUI()
        {
            base.drawGUI();

            GUILayout.Label("tab : " + GetTabLabel());

            tabC = GUILayout.Toggle(tabC, "toggle C");
        }
    }
}