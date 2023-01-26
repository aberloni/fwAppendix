using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace fwp.industries
{
    public class WinEdIndusMgr : EditorWindow
    {
        [MenuItem("Window/Industries/(window) indus factories", false, 1)]
        static void init()
        {
            EditorWindow.GetWindow(typeof(WinEdIndusMgr));
        }

        FactoryBase[] factos;
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
            if(factos == null || force)
            {
                factos = IndusMgr.getAllFactories();
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

            if (GUILayout.Button("refresh content"))
            {
                refreshRefs(true);
            }

            if(factos.Length <= 0)
            {
                GUILayout.Label("has 0 factos");
                return;
            }

            GUILayout.Label("x" + factos.Length + " factories");

            scroll = GUILayout.BeginScrollView(scroll);

            for (int i = 0; i < factos.Length; i++)
            {
                toggleSections[i] = drawFacto(factos[i], toggleSections[i]);
            }

            GUILayout.EndScrollView();
            //EditorGUILayout.ObjectField("Title", objectHandle, typeof(objectClassName), true);
        }

        static private bool drawFacto(FactoryBase facto, bool toggleState)
        {
            var refs = facto.getActives();

            string nm = facto.ToString();
            nm += " x" + refs.Count;

            toggleState = EditorGUILayout.Foldout(toggleState, nm, true);

            if (!toggleState) return false;

            foreach (var elmt in refs)
            {
                if (elmt == null)
                {
                    GUILayout.Label("null");
                    continue;
                }

                GUILayout.BeginHorizontal();

                MonoBehaviour mono = elmt as MonoBehaviour;
                if (mono != null) EditorGUILayout.ObjectField(mono.name, mono, typeof(MonoBehaviour), true);
                else GUILayout.Label(elmt.GetType().ToString());

                GUILayout.EndHorizontal();
            }

            return true;
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
