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

        /// <summary>
        /// called on window creation
        /// </summary>
        void OnEnabled()
        {
            refTypes = IndusReferenceMgr.getAllTypes();
        }

        Type[] refTypes;
        bool[] toggleTypes;

        MonoBehaviour cursorMono = null;

        Vector2 _cursorPosition = Vector2.zero;
        Vector2 cursorPosition = Vector2.zero;

        Vector2 scroll;

        GUIStyle foldTitle = new GUIStyle();

        private void Update()
        {
            updateRefs();
        }

        void updateRefs(bool force = false)
        {
            if (refTypes == null || force)
            {
                refTypes = IndusReferenceMgr.getAllTypes();
                toggleTypes = new bool[refTypes.Length];
            }
        }

        void OnGUI()
        {
            if (!Application.isPlaying)
            {
                GUILayout.Label("at runtime only");
                refTypes = null;
                return;
            }

            /*
            GUILayout.BeginHorizontal();
            GUILayout.Label(cursorPosition.ToString());
            if (cursorMono != null) GUILayout.Label(cursorMono.name.ToString());
            else GUILayout.Label("nothing close");
            GUILayout.EndHorizontal();
            */

            if (GUILayout.Button("uber refresh types list"))
            {
                IndusReferenceMgr.edRefresh(); // ed window
                IndusReferenceMgr.refreshAll();
                updateRefs(true);
            }

            if (refTypes == null)
            {
                GUILayout.Label("no types, plz refresh");
                return;
            }
            if (refTypes.Length <= 0)
            {
                GUILayout.Label("types count 0, plz refresh");
                return;
            }

            scroll = GUILayout.BeginScrollView(scroll);

            GUILayout.Label("x" + refTypes.Length + " in facebook");
            for (int i = 0; i < refTypes.Length; i++)
            {
                toggleTypes[i] = drawListType(refTypes[i], toggleTypes[i]);
            }

            GUILayout.EndScrollView();
            //EditorGUILayout.ObjectField("Title", objectHandle, typeof(objectClassName), true);
        }

        static private bool drawListType(Type typ, bool toggleState)
        {
            List<iIndusReference> refs = IndusReferenceMgr.getGroupByType(typ);

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
