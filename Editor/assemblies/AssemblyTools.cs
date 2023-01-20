using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

using UnityEditor;

namespace fwp.appendix.assembly
{

    static public class AssemblyTools
    {
        public static void ClearConsole()
        {
            var assembly = Assembly.GetAssembly(typeof(SceneView));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }

        static void setAnimationViewSelection(GameObject obj)
        {

            ClearConsole();

            Type t = getAssemblyType("UnityEditor", "AnimationWindow");
            var windowList = invokeMethod("UnityEditor", "AnimationWindow", "GetAllAnimationWindows") as IList;
            var windowMain = windowList[0];
            var winType = windowMain.GetType();

            //logAllMethods(windowMain);
            var title = winType.GetMethod("get_title").Invoke(windowMain, new object[] { });
            Debug.Log(title.ToString());

            FieldInfo windowInfo = t.GetField("m_AnimEditor", BindingFlags.NonPublic | BindingFlags.Instance);
            var animEditor = windowInfo.GetValue(windowMain);
            //Debug.Log(animEditor);

            var selection = animEditor.GetType().GetMethod("get_selection").Invoke(animEditor, new object[] { });
            Debug.Log(selection);

            //FieldInfo windowInfo = t.GetField("m_AnimEditor", BindingFlags.NonPublic | BindingFlags.Instance);
            //var animEditor = windowInfo.GetValue(windowMain);

            //var animType = animEditor.GetType();
            //logMethods(windowMain);

            //https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Animation/AnimationWindow/AnimationWindowState.cs
            //windowMain.GetType().GetMethod("EditGameObject").Invoke(windowMain, new object[] { obj });

            Type tSelectionItem = getAssemblyType("UnityEditor", "GameObjectSelectionItem");
            Type taSelectionItem = getAssemblyType("UnityEditor", "AnimationWindowSelectionItem");

            //Debug.Log(tSelectionItem);
            //logAllMethods(taSelectionItem);

            logProperties(animEditor, BindingFlags.NonPublic | BindingFlags.Instance);

            PropertyInfo info = animEditor.GetType().GetProperty("m_State", BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Log(info);
            //var state = animEditor.GetType().GetProperty("m_State", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(animEditor);

            //var taName = animEditor.GetType().GetMethod("get_Name", BindingFlags.Public | BindingFlags.Instance).Invoke(selection, new object[] { });
            //var taName = state.GetType().GetMethod("get_Name", BindingFlags.Public | BindingFlags.Instance).Invoke(state, new object[] { });
            //Debug.Log(taName);

            //var title = taSelectionItem.GetType().GetMethod("get_title")
            //Highlighter.Highlight("Animation", , HighlightSearchMode.None);

            //var tsItem = tSelectionItem.GetMethod("Create", BindingFlags.Public | BindingFlags.Static).Invoke(tSelectionItem, new object[] { obj });

            //logMethods(windowMain, BindingFlags.NonPublic | BindingFlags.Instance);
            //logAllMethods(windowMain);

            //winType.GetMethod("ShouldUpdateGameObjectSelection", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(windowMain, new object[] { tsItem });
        }

        static public void logProperties(object instance, BindingFlags bindings)
        {
            var fs = instance.GetType().GetFields(bindings);
            foreach (var f in fs)
            {
                Debug.Log("  " + f);
            }
        }

        static public void logMethods(object instance, BindingFlags bindings)
        {
            var type = instance.GetType();
            var myFields = type.GetMethods(bindings);
            Debug.Log("logging methods of " + type + " methods found : " + myFields.Length);
            foreach (var info in myFields) Debug.Log("  " + info.Name);
        }
        static public void logAllMethods(object instance)
        {
            var type = instance.GetType();

            Debug.Log("logging methods of " + type);

            var myFields = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            Debug.Log("public | static : " + myFields.Length);
            foreach (var info in myFields) Debug.Log("  " + info.Name);

            myFields = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
            Debug.Log("non public | static : " + myFields.Length);
            foreach (var info in myFields) Debug.Log("  " + info.Name);

            myFields = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            Debug.Log("public | instance : " + myFields.Length);
            foreach (var info in myFields) Debug.Log("  " + info.Name);

            myFields = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Log("non public | instance : " + myFields.Length);
            foreach (var info in myFields) Debug.Log("  " + info.Name);
        }

        static public Type getAssemblyType(string assemblyName, string className)
        {
            var ass = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in ass)
            {
                if (assembly.GetName().ToString().Contains(assemblyName))
                {
                    //Debug.Log(assembly.GetName().ToString());

                    Type[] ts = assembly.GetTypes();
                    foreach (var t in ts)
                    {

                        if (t.ToString().Contains(className)) return t;
                    }
                }
            }
            return null;
        }

        static public object invokeMethod(string assemblyName, string className, string methodName)
        {
            var ass = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in ass)
            {
                if (assembly.GetName().ToString().Contains(assemblyName))
                {
                    Debug.Log(assembly.GetName().ToString());

                    Type[] ts = assembly.GetTypes();
                    foreach (var t in ts)
                    {

                        if (t.ToString().Contains(className))
                        {
                            Debug.Log("  -> " + t.ToString());

                            MethodInfo method = t.GetMethod(methodName);
                            return method.Invoke(null, null);
                            //Debug.Log(output);

                            //t.InvokeMember(method.Name, BindingFlags.InvokeMethod, new Type[] { });

                        }
                    }
                }
            }
            return null;
        }

        static public void search(string filter)
        {
            ClearConsole();

            //System.Reflection.Assembly[] ass = System.AppDomain.CurrentDomain.GetAssemblies();
            var ass = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in ass)
            {
                Type[] ts = assembly.GetTypes();

                if (assembly.FullName.Contains(filter))
                {
                    //Debug.Log("<b>" + assembly + "</b> Types counts : " + ts.Length);

                    foreach (Type t in ts)
                    {
                        //Debug.Log("  L " + t);

                        if (typeof(ScriptableObject).IsAssignableFrom(t))
                        {
                            var list = t.GetCustomAttributes(false);
                            foreach (var l in list)
                            {
                                if (l is RequireComponent) Debug.Log("  L " + l);
                            }
                        }
                    }
                }
            }

        }

    }

}