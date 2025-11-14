using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.utils.editor
{
	/// <summary>
	/// gives this view the tools to filter content
	/// </summary>
	public class WinEdFilterable : WinEdWrapper
	{
		string filter = string.Empty;

		public bool HasFilter => filter.Length > 0;
		public string Filter => filter;

		public System.Action<string> onFilterValueChanged;

		const string label_filter = "filter";
		const string label_clear = "clear";

		protected void drawFilterField()
		{
			GUILayout.BeginHorizontal();

			GUILayout.Label(label_filter, GUILayout.Width(50f));
			var _filter = GUILayout.TextArea(filter);

			if (GUILayout.Button(label_clear, GUILayout.Width(50f)))
			{
				filter = string.Empty;
			}

			if (_filter != filter)
			{
				filter = _filter;
				onFilterValueChanged(filter);
			}

			GUILayout.EndHorizontal();
		}

		virtual protected void reactFilterValueChanged(string newValue)
		{
			onFilterValueChanged?.Invoke(newValue);
		}
	}

}
