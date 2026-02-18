using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.scenes.examples
{

    public class CustomSceneSub : SceneSubFolder
    {
        public CustomSceneSub(string folderPath, SceneProfil[] profils) : base(folderPath, profils)
        {
        }

        protected override bool drawLineContent(SceneProfil profil)
        {
            bool output = base.drawLineContent(profil);

            //bool present = SceneTools.isEditorSceneLoaded(profil.uid);
            //if(!present) GUILayout.Label("!");
            var sc = profil.extractMainScene();
            if (sc != null)
            {
                GUILayout.Label(sc.Value.name);
            }

            return output;
        }
    }
}