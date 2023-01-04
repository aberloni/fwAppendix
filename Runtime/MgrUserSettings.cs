using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace fwp.appendix.user
{
    static public class MgrUserSettings
    {

        static public void setEdBool(string uid, bool val)
        {
#if UNITY_EDITOR
            EditorPrefs.SetBool(uid, val);
#endif

            log(uid, val);
        }

        static public bool getEdBool(string uid)
        {
#if UNITY_EDITOR
            return EditorPrefs.GetBool(uid, false);
#else
            return false;
#endif
        }


        static public void setEdFloat(string uid, float val)
        {
#if UNITY_EDITOR
            EditorPrefs.SetFloat(uid, val);
#endif

            log(uid, val);
        }

        static public float getEdFloat(string uid, float def)
        {
#if UNITY_EDITOR
            return EditorPrefs.GetFloat(uid, def);
#else
            return 0f;
#endif
        }

        static public int getEdInt(string uid, int def)
        {
#if UNITY_EDITOR
            return EditorPrefs.GetInt(uid, def);
#else
            return 0;
#endif
        }

        static public void setEdInt(string uid, int val)
        {
#if UNITY_EDITOR
            EditorPrefs.SetInt(uid, val);
#endif

            log(uid, val);
        }


        static public string getEdString(string uid, string def)
        {
#if UNITY_EDITOR
            return EditorPrefs.GetString(uid, def);
#else
            return string.Empty;
#endif
        }

        static public void setEdString(string uid, string val)
        {
#if UNITY_EDITOR
            EditorPrefs.SetString(uid, val);
#endif

            log(uid, val);
        }

        static void log(string uid, object val)
        {
            Debug.Log("edUserSet:" + uid + ":" + val);
        }

    }
}