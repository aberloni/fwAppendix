using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// gives this view the tools to filter content
/// </summary>
public class WinEdFilterable : WinEdWrapper
{
    string _filter = string.Empty;

    protected bool hasFilter => _filter.Length > 0;
    protected string filter => _filter;

    protected void drawFilterField()
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("filter", GUILayout.Width(50f));
        _filter = GUILayout.TextArea(_filter);

        if (GUILayout.Button("clear", GUILayout.Width(50f)))
            _filter = string.Empty;

        GUILayout.EndHorizontal();
    }

}
