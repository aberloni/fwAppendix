using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.utils.editor
{
		
	static public class QuickEditorViewStyles
	{
        public const float btnS = 30f;
        public const float btnM = 50f;
        public const float btnL = 75f;
        public const float btnXL = 100f;

		public static readonly GUIStyle TextActive;
		public static readonly GUIStyle text_inactive;
		public static readonly GUIStyle gWinTitleButton;
		public static readonly GUIStyle gWinTitle;

		public static readonly GUIStyle gSectionTitle;

		public static readonly GUIContent gQuestionMark = new GUIContent("?");
		public static readonly GUIContent gPlus = new GUIContent("+");
		public static readonly GUIContent gMinus = new GUIContent("-");

		static QuickEditorViewStyles()
		{
			TextActive = new GUIStyle();
			TextActive.normal.textColor = Color.green;

			text_inactive = new GUIStyle();
			text_inactive.normal.textColor = Color.red;

			gWinTitleButton = new GUIStyle(GUI.skin.button);
			gWinTitleButton.richText = true;
			gWinTitleButton.alignment = TextAnchor.MiddleCenter;
			gWinTitleButton.fontSize = 15;
			gWinTitleButton.margin = new RectOffset(5, 5, 10, 10);
			gWinTitleButton.fixedWidth = 40f;

			gWinTitle = new GUIStyle();
			gWinTitle.richText = true;
			gWinTitle.alignment = TextAnchor.MiddleCenter;
			gWinTitle.normal.textColor = Color.white;
			gWinTitle.fontSize = 20;
			gWinTitle.fontStyle = FontStyle.Bold;
			gWinTitle.margin = new RectOffset(10, 0, 10, 10);

			gSectionTitle = new GUIStyle();
			gSectionTitle.richText = true;
			gSectionTitle.alignment = TextAnchor.MiddleLeft;
			gSectionTitle.normal.textColor = Color.white;
			gSectionTitle.fontStyle = FontStyle.Bold;
			gSectionTitle.margin = new RectOffset(10, 10, 10, 10);
		}

	}
	
}