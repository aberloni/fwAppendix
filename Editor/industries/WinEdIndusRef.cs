using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace fwp.industries
{
    public class WinEdIndusRef : EditorWindow
    {
        [MenuItem("Window/Industries/(window) indus ref", false, 1)]
        static void init()
        {
            EditorWindow.GetWindow(typeof(WinEdIndusRef));
        }

        List<Type> refTypes;
        bool[] toggleTypes;

        //MonoBehaviour cursorMono = null;

        Vector2 _cursorPosition = Vector2.zero;
        Vector2 cursorPosition = Vector2.zero;

        Vector2 scroll;

        private void Update()
        {
            updateRefs();
        }

        void updateRefs(bool force = false)
        {
            if (refTypes == null || force)
            {
                refTypes = IndusReferenceMgr.instance.GetAllTypes();
                toggleTypes = new bool[refTypes.Count];
            }
        }

        void OnGUI()
        {
            if (refTypes == null)
            {
                GUILayout.Label("no types");
                return;
            }

            if (GUILayout.Button("refresh types(s)"))
            {
                updateRefs(true);
            }

            if (refTypes.Count <= 0)
            {
                GUILayout.Label("facebook has 0 type(s)");
                return;
            }

            GUILayout.Label("x" + refTypes.Count + " in facebook");

            if (GUILayout.Button("refresh list(s)"))
            {
                IndusReferenceMgr.instance.RefreshAll();
            }

            scroll = GUILayout.BeginScrollView(scroll);

            for (int i = 0; i < refTypes.Count; i++)
            {
                toggleTypes[i] = drawListType(refTypes[i], toggleTypes[i]);
            }

            GUILayout.EndScrollView();
            //EditorGUILayout.ObjectField("Title", objectHandle, typeof(objectClassName), true);
        }

        static private bool drawListType(Type typ, bool toggleState)
        {
            var refs = IndusReferenceMgr.instance.GetGroup(typ);

            string nm = typ.ToString();
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
