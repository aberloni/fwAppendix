using fwp.utils.editor.tabs;
using UnityEditor;

namespace fwp.examples
{

    public class NestedMain : WinEdTabs
    {

        [UnityEditor.MenuItem("Screen/scenes", false, 1)]
        static void init() => GetWindow(typeof(NestedMain));

        public override void populateTabsEditor(WrapperTabs wt)
        {
            wt.addSpecificTab(new NTabA());
            wt.addSpecificTab(new NTabsB("Tabs nested (B)", "subnest"));
        }
    }

}