using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using fwp.appendix;
using fwp.appendix.user;
using fwp.scenes;

/// <summary>
/// gather all scenes profiles for a specific folder
/// scenes[] will be override externaly
/// permet de regrouper les sceneprofil dans un même container
/// </summary>
public class SceneSubFolder
{
    public string projectPath; // where the folder is located in Assets/
    public string folderName; // just folder name

    public string completePath => System.IO.Path.Combine(projectPath, folderName);

    public List<SceneProfil> scenes;

    public bool toggled
    {
        set
        {
            EditorPrefs.SetBool(completePath, value);
        }
        get
        {
            return EditorPrefs.GetBool(completePath, false);
        }
    }

    public SceneSubFolder(string folderPath)
    {
        projectPath = folderPath;

        if (projectPath.Length <= 0)
        {
            Debug.LogWarning("no base path given ?");
        }

        folderName = folderPath.Substring(folderPath.LastIndexOf("/") + 1);
    }

    public bool hasContent(string filter)
    {
        if (filter.Length <= 0)
            return scenes.Count > 0;

        int cnt = 0;
        for (int i = 0; i < scenes.Count; i++)
        {
            if (scenes[i].label.Contains(filter))
                cnt++;
        }

        return cnt > 0;
    }
    public void drawSection(string filter)
    {

        if (!hasContent(filter))
        {
            GUILayout.Label(folderName);
        }
        else
        {

            // sub folder
            toggled = EditorGUILayout.Foldout(toggled, folderName + " (x" + scenes.Count + ")", true);
            if (toggled)
            {
                foreach (var profil in scenes)
                {
                    drawSceneLine(profil);
                }
            }
        }

    }

    virtual protected void logSceneDetails(SceneProfil profil)
    {
        Debug.Log("profil:" + profil.label);

        Debug.Log("  -> layers x" + profil.layers.Count);
        foreach (var elmt in profil.layers)
            Debug.Log(elmt);

        Debug.Log("  -> deps x" + profil.deps.Count);
        foreach (var dep in profil.deps)
            Debug.Log(dep);

        // and ping scene
        GuiHelpers.pingScene(profil.pathToScene);
    }

    /// <summary>
    /// root call to draw the line of given profil
    /// </summary>
    protected bool drawSceneLine(SceneProfil profil)
    {
        GUILayout.BeginHorizontal();

        bool output = drawLineContent(profil);

        GUILayout.EndHorizontal();

        return output;
    }

    /// <summary>
    /// whatever is drawn in a profil line
    /// true : pressed button & load is called
    /// </summary>
    virtual protected bool drawLineContent(SceneProfil profil)
    {
        if (GUILayout.Button("?", GUILayout.Width(GuiHelpers.btnSymbWidthSmall)))
        {
            logSceneDetails(profil);
        }

        bool load = false;

        // scene button
        if (GUILayout.Button(profil.editor_getButtonName())) // each profil
        {
            //if (EditorPrefs.GetBool(edLoadDebug)) section[i].loadDebug = true;
            //profil.editorLoad(false);
            onEditorSceneCall(profil, true);
            load = true;
        }

        // add/remove buttons
        bool present = SceneTools.isEditorSceneLoaded(profil.label);
        //bool present = profil.isLoaded();
        string label = present ? "-" : "+";

        if (GUILayout.Button(label, GUILayout.Width(GuiHelpers.btnSymbWidth)))
        {
            if (!present)
            {
                onEditorSceneCall(profil, false);
                reactSceneCall(profil, true);
                load = true;
            }
            else
            {
                onEditorSceneRemoval(profil);
                reactSceneCall(profil, false);
            }
        }

        return load;
    }

    /// <summary>
    /// when user calls for a scene
    /// load or unload
    /// </summary>
    virtual protected void reactSceneCall(SceneProfil profil, bool load)
    { }

    virtual public string stringify()
    {
        //return "@path:" + projectPath + " @folder:" + folderName + ", total scenes x" + scenes.Count;
        return "@folder:" + folderName + ", total scenes x" + scenes.Count;
    }

    public const string _pref_autoAdd = "scenesAutoAdd";

    /// <summary>
    /// additive only for loading
    /// </summary>
    void onEditorSceneCall(SceneProfil profil, bool replaceContext)
    {
        profil.setDirty();
        profil.editorLoad(replaceContext, MgrUserSettings.getEdBool(_pref_autoAdd));
    }

    void onEditorSceneRemoval(SceneProfil profil)
    {
        profil.setDirty();
        profil.editorUnload();
    }

    /// <summary>
    /// shk
    /// </summary>
    static public void drawAutoAdd()
    {
        EdUserSettings.drawBool("+ build settings", _pref_autoAdd);
    }



}
