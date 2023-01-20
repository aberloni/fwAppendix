using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace fwp.appendix.assembly
{
    public class WinEdAssemblyExplorer : EditorWindow
    {
        [MenuItem("Window/Assembly lookup")]
        static void init()
        {
            WinEdAssemblyExplorer window = (WinEdAssemblyExplorer)GetWindow(typeof(WinEdAssemblyExplorer));
            window.Show();
            //EditorWindow.GetWindow(typeof(AssemblyEditorWindow));
        }

        public struct AssemblyEditorView
        {
            public Assembly ass;
            public bool dropState;
            public Vector2 scrollPosition;
        }

        string filterAssemblyField = "UnityEditor";
        string filterTypeField = "";

        List<AssemblyEditorView> views = new List<AssemblyEditorView>();

        Vector2 scroll;
        void OnGUI()
        {
            GUILayout.Label("filters");

            GUILayout.BeginHorizontal();
            filterAssemblyField = EditorGUILayout.TextField("assembly", filterAssemblyField);

            if (GUILayout.Button("explore", GUILayout.Width(100f)))
            {
                views.Clear();

                //System.Reflection.Assembly[] ass = System.AppDomain.CurrentDomain.GetAssemblies();
                var ass = System.AppDomain.CurrentDomain.GetAssemblies();

                foreach (var assembly in ass)
                {
                    Type[] ts = assembly.GetTypes();

                    if (assembly.FullName.Contains(filterAssemblyField))
                    {
                        views.Add(new AssemblyEditorView() { ass = assembly, dropState = false });
                    }
                }
                Debug.Log(views.Count);
            }
            GUILayout.EndHorizontal();

            if (views.Count <= 0) return;

            GUILayout.Space(10f);
            filterTypeField = EditorGUILayout.TextField("filter", filterTypeField);

            scroll = EditorGUILayout.BeginScrollView(scroll);

            for (int i = 0; i < views.Count; i++)
            {
                AssemblyEditorView view = views[i];
                view.dropState = drawAssemblyData(view).dropState;
                views[i] = view;
            }

            EditorGUILayout.EndScrollView();


        }

        AssemblyEditorView drawAssemblyData(AssemblyEditorView view)
        {
            //https://docs.unity3d.com/ScriptReference/EditorGUILayout.Foldout.html

            Type[] ts = view.ass.GetTypes();

            view.dropState = EditorGUILayout.Foldout(view.dropState, view.ass.GetName().ToString() + " (" + ts.Length + ")");

            if (!view.dropState) return view;

            //Debug.Log("<b>" + view.ass.GetName().ToString() + "</b> Types counts : " + ts.Length);
            //view.scrollPosition = EditorGUILayout.BeginScrollView(view.scrollPosition, false, true);

            foreach (Type t in ts)
            {
                if (t.ToString().Contains(filterTypeField))
                {
                    GUILayout.Label(t.ToString());

                    t.GetProperties();

                    PropertyInfo[] props = t.GetProperties();

                    EditorGUILayout.BeginHorizontal();
                    foreach (PropertyInfo info in props)
                    {
                        GUILayout.Label(info.Name);
                    }
                    EditorGUILayout.EndHorizontal();

                }

            }
            //EditorGUILayout.EndScrollView();

            return view;
        }

    }
}