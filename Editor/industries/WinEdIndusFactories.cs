using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace fwp.industries
{
    /// <summary>
    /// view to visualize content of Factories
    /// </summary>
    public class WinEdIndusFactories : EditorWindow
    {
        [MenuItem("Window/Industries/(window) indus factories", false, 1)]
        static void init()
        {
            EditorWindow.GetWindow(typeof(WinEdIndusFactories));
        }

        IFactory[] factos;
        bool[] toggleSections;

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
                toggleSections = new bool[factos.Length];
            }
        }

        void OnGUI()
        {
            if (factos == null)
            {
                GUILayout.Label("no factors");
                return;
            }

            if (GUILayout.Button("refresh factory content"))
            {
                refreshRefs(true);
                return;
            }

            GUILayout.Label("factories");

            if (factos.Length <= 0)
            {
                GUILayout.Label("empty");
                return;
            }

            GUILayout.Label("x" + factos.Length + " factories / " + toggleSections.Length);

            scroll = GUILayout.BeginScrollView(scroll);

            for (int i = 0; i < factos.Length; i++)
            {
                toggleSections[i] = drawFacto(factos[i], toggleSections[i]);
            }

            GUILayout.EndScrollView();
            //EditorGUILayout.ObjectField("Title", objectHandle, typeof(objectClassName), true);
        }

        bool drawFacto(IFactory facto, bool toggleState)
        {
            toggleState = EditorGUILayout.Foldout(toggleState, facto.GetType().ToString(), true);

            if (toggleState)
            {

                if (GUILayout.Button("recycle actives"))
                {
                    facto.recycleAll();
                }

                drawFactoList("actives", facto.edGetActives());
                drawFactoList("inactives", facto.edGetInactives());

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
