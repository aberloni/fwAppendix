using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using fwp.settings.editor;

/// <summary>
/// EditorGUILayout.Popup("startup room", index, roomLabels);
/// </summary>

namespace fwp.appendix.user
{

#if UNITY_EDITOR

    static public class EdUserSettings
    {

        static public bool drawBool(string label, string uid, Action<bool> onChange = null)
        {
            bool val = MgrEdUserSettings.getBool(uid);
            bool _val = EditorGUILayout.Toggle(label, val);
            if(val != _val)
            {
				MgrEdUserSettings.setBool(uid, _val);
                onChange?.Invoke(_val);
            }
            return _val;
        }

        static public float drawSlider(string label, string uid, Vector2 range, Action<float> onChange = null)
        {
            float val = MgrEdUserSettings.getFloat(uid, 1f);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label} : {val.ToString("0.00")}");

            //Vector2 minmax = new Vector2(0.1f, 1f);
            GUILayout.Label(range.x.ToString(), GUILayout.Width(30f));
            float _val = GUILayout.HorizontalSlider(val, range.x, range.y);
            if(_val != val)
            {
				MgrEdUserSettings.setFloat(uid, _val);
                onChange?.Invoke(_val);
            }
            GUILayout.Label(range.y.ToString(), GUILayout.Width(30f));

            GUILayout.EndHorizontal();

            return _val;
        }

    }

#endif
}
