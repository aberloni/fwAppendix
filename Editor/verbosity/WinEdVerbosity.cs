using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

namespace fwp.verbosity
{
    public class WinEdVerbosity : EditorWindow
    {
        [MenuItem("Window/(window) verbosity")]
        static protected void init()
        {
            EditorWindow.GetWindow<WinEdVerbosity>();
        }

        Enum[] keys;

        private void OnEnable()
        {
            refresh(true);
        }

        private void OnFocus()
        {
            refresh(true);
        }

        void refresh(bool force)
        {
            if (keys == null || force)
            {
                keys = injectKeys().ToArray();
            }
        }

        virtual protected List<Enum> injectKeys()
        {
            var ret = new List<Enum>();

            Type t = typeof(VerbositySectionUniversal);
            var enumValue = (Enum)System.Activator.CreateInstance(t);

            ret.Add(enumValue);
            return ret;
        }

        private void OnGUI()
        {
            if (keys == null)
            {
                GUILayout.Label("view not ready");
                return;
            }

            GUILayout.Label("toggles x" + keys.Length);

            // each possible enums
            foreach (var key in keys)
            {
                //GUILayout.BeginHorizontal();
                //GUILayout.Label(key.ToString());

                Enum pv = Verbosity.getMaskEnum(key);
                Enum nv = EditorGUILayout.EnumFlagsField(key.ToString(), pv);

                if (pv != nv)
                {
                    Verbosity.toggle(nv);
                }

            }

        }

        public static Array GetUnderlyingEnumValues(Type type)
        {
            Array values = Enum.GetValues(type);
            Type underlyingType = Enum.GetUnderlyingType(type);
            Array arr = Array.CreateInstance(underlyingType, values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                arr.SetValue(values.GetValue(i), i);
            }
            return arr;
        }
    }

}
