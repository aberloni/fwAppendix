using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace fwp.localization
{
    using fwp.localization.editor;

    public class LocalizationWindow<Manager> : EditorWindow where Manager : LocalizationManager
    {
        /*
        [MenuItem("Localization/viewer")]
        static void init()
        {
            EditorWindow.GetWindow(typeof(LocalizationWindow));
        }
        */

        string output = string.Empty;

        LocalizationManager mgr;

        virtual public Manager getManager() => LocalizationManager.get() as Manager;

        private void OnGUI()
        {
            Manager mgr = getManager();
            if(mgr == null)
            {
                GUILayout.Label("no manager <"+typeof(Manager)+"> ?");
                return;
            }

            GUILayout.Label(mgr.GetType().ToString());

            draw(mgr);
        }

        virtual protected void draw(Manager mgr)
        {
            if(GUILayout.Button("download"))
            {
                ExportLocalisationToGoogleForm.ssheet_import(mgr.getSheetLabels());
            }

            if (GUILayout.Button("generate trad files"))
            {
                ExportLocalisationToGoogleForm.trad_files_generation();
            }

            if (GUILayout.Button("solve chinese"))
            {
                LocalizationFile file = mgr.getFileByLang(IsoLanguages.zh.ToString());

                output = string.Empty;

                var lines = file.getLines();
                foreach (var line in lines)
                {
                    output += line + "\n";
                }
            }

            GUILayout.TextArea(output);
        }

    }

}
