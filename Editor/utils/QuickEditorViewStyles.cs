using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

static public class QuickEditorViewStyles
{

	static GUIStyle text_active;
	static public GUIStyle getTextGreen()
	{

		if (text_active == null)
		{
			text_active = new GUIStyle();
			text_active.normal.textColor = Color.green;
		}

		return text_active;
	}

	static GUIStyle text_inactive;
	static public GUIStyle getTextRed()
	{

		if (text_inactive == null)
		{
			text_inactive = new GUIStyle();
			text_inactive.normal.textColor = Color.red;
		}

		return text_inactive;
	}

	static private GUIStyle gWinTitleButton;
	static public GUIStyle WinTitleButton
	{
		get
		{
			if (gWinTitleButton != null) return gWinTitleButton;

			gWinTitleButton = new GUIStyle(GUI.skin.button);

			gWinTitleButton.richText = true;
			gWinTitleButton.alignment = TextAnchor.MiddleCenter;
			gWinTitleButton.fontSize = 15;
			gWinTitleButton.margin = new RectOffset(5, 5, 10, 10);
			gWinTitleButton.fixedWidth = 40f;

			return gWinTitleButton;
		}
	}

	static private GUIStyle gWinTitle;
	static public GUIStyle WinTitle
	{
		get
        {
			if (gWinTitle != null) return gWinTitle;
			gWinTitle = new GUIStyle();

			gWinTitle.richText = true;
			gWinTitle.alignment = TextAnchor.MiddleLeft;
			gWinTitle.normal.textColor = Color.white;
			gWinTitle.fontSize = 20;
			gWinTitle.fontStyle = FontStyle.Bold;
			gWinTitle.margin = new RectOffset(10, 0, 10, 10);
			return gWinTitle;
		}
	}

	static private GUIStyle gSectionFoldTitle;
	static public GUIStyle getSectionFoldTitle(TextAnchor anchor = TextAnchor.MiddleLeft, int leftMargin = 10)
	{
		if(gSectionFoldTitle == null)
        {
			gSectionFoldTitle = EditorStyles.foldout;
			gSectionFoldTitle.alignment = anchor;
			gSectionFoldTitle.margin = new RectOffset(leftMargin, 10, 10, 10);
		}
		return gSectionFoldTitle;
		
	}

	static private GUIStyle gSectionTitle;
	static public GUIStyle getSectionTitle(int size = 15, TextAnchor anchor = TextAnchor.MiddleCenter, int leftMargin = 10)
	{
		if (gSectionTitle == null)
		{
			gSectionTitle = new GUIStyle();

			gSectionTitle.richText = true;
			gSectionTitle.alignment = anchor;
			gSectionTitle.normal.textColor = Color.white;

			gSectionTitle.fontStyle = FontStyle.Bold;
			gSectionTitle.margin = new RectOffset(leftMargin, 10, 10, 10);
			//gWinTitle.padding = new RectOffset(30, 30, 30, 30);

		}

		gSectionTitle.fontSize = size;

		return gSectionTitle;
	}
}
