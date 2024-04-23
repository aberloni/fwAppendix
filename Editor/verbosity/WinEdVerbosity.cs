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

            foreach (var t in getInjectionCandidates())
            {
                injectEnum(t, ret);
            }

            return ret;
        }

        virtual protected Type[] getInjectionCandidates() => new Type[] { typeof(VerbositySectionUniversal) };

        protected void injectEnum(Type t, List<Enum> list)
        {
            var enumValue = (Enum)System.Activator.CreateInstance(t);
            list.Add(enumValue);
        }

        protected void injectEnum<E>(List<Enum> list) where E : Enum => injectEnum(typeof(E), list);

        private void OnGUI()
        {
            if (keys == null)
            {
                GUILayout.Label("view not ready");
                return;
            }

            GUILayout.Label("Verbosity toggles (x" + keys.Length + ")");

            // each possible enums
            foreach (var key in keys)
            {
                //GUILayout.BeginHorizontal();
                //GUILayout.Label(key.ToString());

                Enum pv = Verbosity.getMaskEnum(key);

                var label = key.GetType().Name;
                Enum nv = EditorGUILayout.EnumFlagsField(label, pv);

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
