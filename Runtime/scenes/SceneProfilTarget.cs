using UnityEngine;
using fwp.scenes;

public class SceneProfilTarget
{

    string sceneName = string.Empty;
    int _order = 0;

    public int Order => _order;
    public string Name => sceneName;

    public bool IsLoaded
    {
        get
        {
            return UnityEngine.SceneManagement.SceneManager.GetSceneByName(Name).isLoaded;
        }
    }

    public SceneProfilTarget(string nm, int ord)
    {
        sceneName = nm;
        _order = ord;
    }

    public bool Contains(string filter)
    {
        return sceneName.ToLower().Contains(filter);
    }

    public void editorUnload()
    {
         SceneLoaderEditor.unloadScene(Name);
    }

    public bool HasPriorityOver(SceneProfilTarget other)
    {
        return other.Order < _order;
    }

    public bool IsPriority(string suffix)
    {
        return Name.EndsWith(suffix);
    }
}
