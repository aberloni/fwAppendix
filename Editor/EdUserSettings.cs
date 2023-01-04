using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// EditorGUILayout.Popup("startup room", index, roomLabels);
/// </summary>

namespace fwp.appendix.user
{

    static public class EdUserSettings
    {

        static public void drawBool(string label, string uid)
        {
            bool val = MgrUserSettings.getEdBool(uid);
            bool _val = EditorGUILayout.Toggle(label, val);
            if(val != _val)
            {
                MgrUserSettings.setEdBool(uid, _val);
            }
        }

        static public void drawSlider(string label, string uid, Vector2 range)
        {
            float val = MgrUserSettings.getEdFloat(uid, 1f);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label} : {val.ToString("0.00")}");

            //Vector2 minmax = new Vector2(0.1f, 1f);
            GUILayout.Label(range.x.ToString(), GUILayout.Width(30f));
            float _val = GUILayout.HorizontalSlider(val, range.x, range.y);
            if(_val != val)
            {
                MgrUserSettings.setEdFloat(uid, _val);
            }
            GUILayout.Label(range.y.ToString(), GUILayout.Width(30f));

            GUILayout.EndHorizontal();
        }

    }

}
