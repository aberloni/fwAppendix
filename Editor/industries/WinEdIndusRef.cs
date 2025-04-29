using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace fwp.industries
{
	/// <summary>
	/// view to visualize facebook content
	/// </summary>
	public class WinEdIndusRef : EditorWindow
	{
		[MenuItem("Window/Industries/(window) indus ref", false, 1)]
		static void init()
		{
			EditorWindow.GetWindow(typeof(WinEdIndusRef));
		}

		List<Type> refTypes => IndusReferenceMgr.instance.GetAllTypes();
		bool[] toggleTypes;

		Vector2 scroll;

		void updateRefs(bool force = false)
		{
			if (toggleTypes == null || force)
			{
				toggleTypes = new bool[refTypes.Count];
			}
		}

		private void OnFocus()
		{
			updateRefs(true);
		}

		void OnGUI()
		{
			if (toggleTypes == null)
			{
				GUILayout.Label("no types");
				return;
			}

			if (GUILayout.Button("refresh facebook type(s)"))
			{
				updateRefs(true);
			}

			GUILayout.Label("facebook");

			if (toggleTypes.Length <= 0)
			{
				GUILayout.Label("empty type(s)");
				return;
			}

			GUILayout.Label("types  x" + toggleTypes.Length + "");

			if (GUILayout.Button("reinject facebook list(s) using context monos"))
			{
				IndusReferenceMgr.instance.RefreshAll();
			}

			scroll = GUILayout.BeginScrollView(scroll);

			for (int i = 0; i < refTypes.Count; i++)
			{
				if (refTypes[i] == null) GUILayout.Label("null type ?");
				else toggleTypes[i] = drawListType(refTypes[i], toggleTypes[i]);
			}

			GUILayout.EndScrollView();
			//EditorGUILayout.ObjectField("Title", objectHandle, typeof(objectClassName), true);
		}

		static private bool drawListType(Type typ, bool toggleState)
		{
			var refs = IndusReferenceMgr.instance.GetGroup(typ);

			string nm = typ.ToString();
			nm += " x" + refs.Count;

			EditorGUI.BeginChangeCheck();

			toggleState = EditorGUILayout.Foldout(toggleState, nm, true);

			if (EditorGUI.EndChangeCheck())
			{
				//...
			}

			if (toggleState)
			{
				if (GUILayout.Button("fetch from monos")) IndusReferenceMgr.instance.Refresh(typ);

				foreach (var elmt in refs)
				{
					if (elmt == null)
					{
						GUILayout.Label("null");
						continue;
					}

					GUILayout.BeginHorizontal();

					MonoBehaviour mono = elmt as MonoBehaviour;
					if (mono != null) EditorGUILayout.ObjectField(mono.name, mono, typeof(MonoBehaviour), true);
					else GUILayout.Label(elmt.GetType().ToString());

					GUILayout.EndHorizontal();
				}
			}

			return toggleState;
		}


		static private GUIStyle gCategoryBold;
		static public GUIStyle getCategoryBold()
		{
			if (gCategoryBold == null)
			{
				gCategoryBold = new GUIStyle();
				gCategoryBold.normal.textColor = new Color(1f, 0.5f, 0.5f); // red ish
				gCategoryBold.fontStyle = FontStyle.Bold;
			}
			return gCategoryBold;
		}

	}

}
