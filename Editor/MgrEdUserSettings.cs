using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.settings.editor
{
	using UnityEditor;

	/// <summary>
	/// https://discussions.unity.com/t/editor-how-to-add-checkmarks-to-menuitems/114525
	/// https://docs.unity3d.com/ScriptReference/Menu.SetChecked.html
	/// </summary>
	static public class MgrEdUserSettings
	{
		const string ed_user_settings = "Tools/Appendix/User settings verbose";

		[MenuItem(ed_user_settings)]
		static public void toggleUserSettingsVerbose()
		{
			Menu.SetChecked(ed_user_settings, !Verbose);
			Debug.Log("user.settings.verbose : " + Verbose);
		}

		static bool Verbose => Menu.GetChecked(ed_user_settings);

		static public bool getBool(string uid) => EditorPrefs.GetFloat(uid, 0f) > 0f;
		static public void setBool(string uid, bool val)
		{
			EditorPrefs.SetFloat(uid, val ? 1f : 0f);
			log(uid, val);
		}

		static public float getFloat(string uid, float def = 0f) => EditorPrefs.GetFloat(uid, def);
		static public void setFloat(string uid, float val)
		{
			EditorPrefs.SetFloat(uid, val);
			log(uid, val);
		}

		static public int getInt(string uid, int def = 0) => EditorPrefs.GetInt(uid, def);
		static public void setInt(string uid, int val)
		{
			EditorPrefs.SetInt(uid, val);
			log(uid, val);
		}

		static public string getString(string uid, string def = "") => EditorPrefs.GetString(uid, def);
		static public void setString(string uid, string val)
		{
			EditorPrefs.SetString(uid, val);
			log(uid, val);
		}

		static void log(string uid, object val)
		{
			if (!Verbose) return;
			Debug.Log("edSettings:" + uid + ":" + val, val as UnityEngine.Object);
		}

	}
}