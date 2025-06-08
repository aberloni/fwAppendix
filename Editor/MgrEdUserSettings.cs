using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.settings.editor
{
	using UnityEditor;
	using fwp.settings;

	static public class MgrEdUserSettings
	{
		static bool Verbose => Application.isEditor;

		static Dictionary<string, List<iSettingBool>> bools = new();
		static Dictionary<string, List<iSettingFloat>> floats = new();
		static Dictionary<string, List<iSettingString>> strings = new();
		static Dictionary<string, List<iSettingInt>> ints = new();

		static public void unsubs(iSetting target)
		{
			if (target is iSettingBool) foreach (var kp in bools) kp.Value.Remove(target as iSettingBool);
			if (target is iSettingFloat) foreach (var kp in floats) kp.Value.Remove(target as iSettingFloat);
			if (target is iSettingInt) foreach (var kp in ints) kp.Value.Remove(target as iSettingInt);
			if (target is iSettingString) foreach (var kp in strings) kp.Value.Remove(target as iSettingString);
		}

		static public void subBool(string uid, iSettingBool target)
		{
			if (!bools.ContainsKey(uid)) bools.Add(uid, new());
			bools[uid].Add(target);
		}

		static public bool getBool(string uid) => EditorPrefs.GetFloat(uid, 1f) > 0f;
		static public void setBool(string uid, bool val)
		{
			EditorPrefs.SetFloat(uid, val ? 1f : 0f);
			log(uid, val);
		}

		static public void subFloat(string uid, iSettingFloat target)
		{
			if (!floats.ContainsKey(uid)) floats.Add(uid, new());
			floats[uid].Add(target);
		}

		static public float getFloat(string uid, float def) => EditorPrefs.GetFloat(uid, def);
		static public void setFloat(string uid, float val)
		{
			EditorPrefs.SetFloat(uid, val);
			log(uid, val);
		}

		static public void subInt(string uid, iSettingInt target)
		{
			if (!ints.ContainsKey(uid)) ints.Add(uid, new());
			ints[uid].Add(target);
		}

		static public int getInt(string uid, int def) => EditorPrefs.GetInt(uid, def);
		static public void setInt(string uid, int val)
		{
			EditorPrefs.SetInt(uid, val);
			log(uid, val);
		}

		static public void subString(string uid, iSettingString target)
		{
			if (!strings.ContainsKey(uid)) strings.Add(uid, new());
			strings[uid].Add(target);
		}

		static public string getString(string uid, string def) => EditorPrefs.GetString(uid, def);
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