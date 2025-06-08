using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace fwp.settings
{
	public interface iSetting
	{ }

	public interface iSettingBool : iSetting
	{
		public void applySettings(string uid, bool val);
	}

	public interface iSettingFloat : iSetting
	{
		public void applySettings(string uid, float val);
	}

	public interface iSettingInt : iSetting
	{
		public void applySettings(string uid, int val);
	}

	public interface iSettingString : iSetting
	{
		public void applySettings(string uid, string val);
	}

	static public class MgrUserSettings
	{
		static bool Verbose => !Application.isEditor;

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

		static public bool getBool(string uid) => PlayerPrefs.GetFloat(uid, 1f) > 0f;
		static public void setBool(string uid, bool val = false)
		{
			if (getBool(uid) == val) return;

			PlayerPrefs.SetFloat(uid, val ? 1f : 0f);
			log(uid, val);
			foreach (var b in bools[uid]) b.applySettings(uid, val);

		}

		static public void subFloat(string uid, iSettingFloat target)
		{
			if (!floats.ContainsKey(uid)) floats.Add(uid, new());
			floats[uid].Add(target);
		}

		static public float getFloat(string uid, float def = 0f) => PlayerPrefs.GetFloat(uid, def);
		static public void setFloat(string uid, float val)
		{
			if (val == getFloat(uid)) return;

			PlayerPrefs.SetFloat(uid, val);
			log(uid, val);
			foreach (var b in floats[uid]) b.applySettings(uid, val);
		}

		static public void subInt(string uid, iSettingInt target)
		{
			if (!ints.ContainsKey(uid)) ints.Add(uid, new());
			ints[uid].Add(target);
		}

		static public int getInt(string uid, int def = 0) => PlayerPrefs.GetInt(uid, def);
		static public void setInt(string uid, int val)
		{
			if (val == getInt(uid)) return;

			PlayerPrefs.SetInt(uid, val);
			log(uid, val);
			foreach (var b in ints[uid]) b.applySettings(uid, val);
		}

		static public void subString(string uid, iSettingString target)
		{
			if (!strings.ContainsKey(uid)) strings.Add(uid, new());
			strings[uid].Add(target);
		}

		static public string getString(string uid, string def = "") => PlayerPrefs.GetString(uid, def);
		static public void setString(string uid, string val)
		{
			if (val == getString(uid)) return;

			PlayerPrefs.SetString(uid, val);
			log(uid, val);
			foreach (var b in strings[uid]) b.applySettings(uid, val);
		}

		static void log(string uid, object val)
		{
			if (!Verbose) return;
			Debug.Log("settings.user	" + uid + ":" + val);
		}

	}
}