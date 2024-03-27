using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace fwp.industries
{
    public class WinEdIndusFactories : EditorWindow
    {
        [MenuItem("Window/Industries/(window) indus factories", false, 1)]
        static void init()
        {
            EditorWindow.GetWindow(typeof(WinEdIndusFactories));
        }

        List<iFactory> factos;
        bool[] toggleSections;

        Vector2 _cursorPosition = Vector2.zero;
        Vector2 cursorPosition = Vector2.zero;

        Vector2 scroll;

        private void OnEnable()
        {
            refreshRefs();
        }

        void refreshRefs(bool force = false)
        {
            if (factos == null || force)
            {
                factos = FactoriesMgr.getAllFactories();
                toggleSections = new bool[factos.Count];
            }
        }

        void OnGUI()
        {
            if (factos == null)
            {
                GUILayout.Label("no factors");
                return;
            }

            if (GUILayout.Button("refresh content"))
            {
                refreshRefs(true);
            }

            if (factos.Count <= 0)
            {
                GUILayout.Label("has 0 factos");
                return;
            }

            GUILayout.Label("x" + factos.Count + " factories");

            scroll = GUILayout.BeginScrollView(scroll);

            for (int i = 0; i < factos.Count; i++)
            {
                toggleSections[i] = drawFacto(factos[i], toggleSections[i]);
            }

            GUILayout.EndScrollView();
            //EditorGUILayout.ObjectField("Title", objectHandle, typeof(objectClassName), true);
        }

        bool drawFacto(iFactory facto, bool toggleState)
        {
            toggleState = EditorGUILayout.Foldout(toggleState, facto.GetType().ToString(), true);

            if (toggleState)
            {

                if (GUILayout.Button("recycle actives"))
                {
                    facto.recycleAll();
                }

                drawFactoList("actives", facto.getActives());
                drawFactoList("inactives", facto.getInactives());

            }

            return toggleState;
        }

        void drawFactoList(string lbl, List<iFactoryObject> refs)
        {
            if (refs.Count <= 0)
            {
                GUILayout.Label("nothing to list");
                return;
            }

            GUILayout.Label(lbl + " x" + refs.Count);

            //GUILayout.Label(refs[0].GetType() + " x" + refs.Count);

            foreach (var elmt in refs)
            {
                if (elmt == null)
                {
                    GUILayout.Label("null");
                    continue;
                }

                GUILayout.BeginHorizontal();

                MonoBehaviour mono = elmt as MonoBehaviour;
                if (mono != null) EditorGUILayout.ObjectField("(mono) " + mono.name, mono, typeof(MonoBehaviour), true);
                else GUILayout.Label(elmt.GetType().ToString() + " !mono");

                GUILayout.EndHorizontal();
            }
        }


        static private GUIStyle gCategoryBold;
        static public GUIStyle getCategoryBold()
        {
            if (gCategoryBold == null)
            {
                gCategoryBold = new GUIStyle();
                gCategoryBold.normal.textColor = new Color(1f, 0.5f, 0.5f); // red ish
                gCategoryBold.fontStyle = FontStyle.Bold;
            }
            return gCategoryBold;
        }

    }

}
