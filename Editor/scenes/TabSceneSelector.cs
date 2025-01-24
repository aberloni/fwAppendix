using UnityEngine;
using System.Collections.Generic;


namespace fwp.scenes.editor
{
    using fwp.utils.editor.tabs;
    using fwp.utils.editor;

    public class TabSceneSelector : WrapperTab
    {
        string path;

        public string Path => path;

        public string PathEnd => path.Substring(path.LastIndexOf("/") + 1);

        WinEdBlueprintScenesSelector Selector => Parent as WinEdBlueprintScenesSelector;

        public TabSceneSelector(WinEdBlueprintScenesSelector window, string path) : base(window)
        {
            this.path = path;
            label = PathEnd;
        }

        protected override void drawGUI()
        {
            base.drawGUI();
            drawSubSectionTab();
        }

        /// <summary>
        /// draw generic tab with scene list
        /// </summary>
        void drawSubSectionTab()
        {
            string subSectionUid = Path;

            if (!Selector.HasSections)
            {
                GUILayout.Label("selector has no sections");
                return;
            }

            var sections = Selector.Sections;

            List<SceneSubFolder> subList = null;

            if (sections.ContainsKey(subSectionUid))
            {
                subList = sections[subSectionUid];
            }

            if (subList == null)
            {
                GUILayout.Label("no sublist #" + subSectionUid);
                return;
            }

            GUILayout.BeginHorizontal();

            GUILayout.Label($"{subSectionUid} has x{subList.Count} sub-sections");

            if (GUILayout.Button("ping folder", GUILayout.Width(GuiHelpers.btnLabelWidth)))
            {
                GuiHelpers.selectFolder(subSectionUid, true);
            }

            if (GUILayout.Button("upfold all", GUILayout.Width(GuiHelpers.btnLabelWidth)))
            {
                for (int i = 0; i < subList.Count; i++)
                {
                    subList[i].toggled = false;
                }
            }

            GUILayout.EndHorizontal();

            for (int i = 0; i < subList.Count; i++)
            {
                subList[i].drawSection(Selector.filter);
            }

        }

    }

}