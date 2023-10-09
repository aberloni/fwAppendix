using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class TabsHelper
{
    static public GUIContent[] generateTabsDatas(string[] labels)
	{
        GUIContent[] modeLabels = new GUIContent[labels.Length];

        //GUILayout.Label("no mode labels (x" + nms.Length + " candidates)");

        for (int i = 0; i < labels.Length; i++)
        {
            modeLabels[i] = new GUIContent(labels[i], "tooltip");
        }

        return modeLabels;
    }

    static public int generateTabsHeader(int tabSelected, GUIContent[] tabs)
	{
        //GUIStyle gs = new GUIStyle(GUI.skin.button)
        //int newTab = GUILayout.Toolbar((int)tabSelected, modeLabels, "LargeButton", GUILayout.Width(toolbarWidth), GUILayout.ExpandWidth(true));
        int newTab = GUILayout.Toolbar((int)tabSelected, tabs, "LargeButton");
        //if (newTab != (int)tabSelected) Debug.Log("changed tab ? " + tabSelected);

        return newTab;
    }
}
