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
        static public bool verbose = false;

        static public void setBool(string uid, bool val)
        {
            PlayerPrefs.SetFloat(uid, val ? 1f : 0f);
            logApp(uid, val);
        }
        static public bool getBool(string uid) => PlayerPrefs.GetFloat(uid, 1f) > 0f;

        static public void setFloat(string uid, float val)
        {
            PlayerPrefs.SetFloat(uid, val);
            logApp(uid, val);
        }

        static public float getFloat(string uid, float def)
        {
            return PlayerPrefs.GetFloat(uid, def);
        }

        static public int getInt(string uid, int def)
        {
            return PlayerPrefs.GetInt(uid, def);
        }

        static public void setInt(string uid, int val)
        {
            PlayerPrefs.SetInt(uid, val);
            logApp(uid, val);
        }


        static public string getString(string uid, string def)
        {
            return PlayerPrefs.GetString(uid, def);
        }

        static public void setString(string uid, string val)
        {
            PlayerPrefs.SetString(uid, val);
            logApp(uid, val);
        }


#if UNITY_EDITOR

        static public void setEdBool(string uid, bool val)
        {
            EditorPrefs.SetBool(uid, val);
            logEd(uid, val);
        }

        static public bool getEdBool(string uid)
        {
            return EditorPrefs.GetBool(uid, false);
        }


        static public void setEdFloat(string uid, float val)
        {
            EditorPrefs.SetFloat(uid, val);
            logEd(uid, val);
        }

        static public float getEdFloat(string uid, float def)
        {
            return EditorPrefs.GetFloat(uid, def);
        }

        static public int getEdInt(string uid, int def)
        {
            return EditorPrefs.GetInt(uid, def);
        }

        static public void setEdInt(string uid, int val)
        {
            EditorPrefs.SetInt(uid, val);
            logEd(uid, val);
        }


        static public string getEdString(string uid, string def)
        {
            return EditorPrefs.GetString(uid, def);
        }

        static public void setEdString(string uid, string val)
        {
            EditorPrefs.SetString(uid, val);
            logEd(uid, val);
        }
#endif

        static void logApp(string uid, object val)
        {
            if(verbose)
                Debug.Log("UserSet:" + uid + ":" + val);
        }

        static void logEd(string uid, object val)
        {
            if(verbose)
                Debug.Log("edUserSet:" + uid + ":" + val);
        }

    }
}