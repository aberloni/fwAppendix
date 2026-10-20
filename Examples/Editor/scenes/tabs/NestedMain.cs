namespace fwp.examples
{
    public class NestedMain : fwp.utils.editor.tabs.WinEdTabs
    {
        [UnityEditor.MenuItem("Window/Appendix/examples/+win tabs")]
        static void init() => GetWindow(typeof(NestedMain));

        public override void populateTabsEditor(fwp.utils.editor.tabs.WrapperTabs wt)
        {
            wt.addSpecificTab(new NTabA());
            wt.addSpecificTab(new NTabsB("Tabs nested (B)", "subnest"));
        }
    }
}