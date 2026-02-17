using UnityEngine;
using fwp.scenes;

/// <summary>
/// Profil Scene wrapper
/// around scene name
/// to simplify sorting
/// </summary>
public struct SceneProfilTarget
{
	public struct PatternOrder
	{
		public string suffix;
		public int order;
	}

	string sceneName;
	int _order;

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
		_order = 0;
		setOrder(ord);
	}

	public void setOrder(int order)
	{
		_order = order;
	}

	public bool Contains(string filter)
	{
		return sceneName.ToLower().Contains(filter);
	}

	public bool HasPriorityOver(SceneProfilTarget other)
	{
		return other.Order < _order;
	}

	public bool IsPriority(string suffix)
	{
		return Name.EndsWith(suffix);
	}

#if UNITY_EDITOR
	public void editorUnload()
	{
		SceneLoaderEditor.unloadScene(Name);
	}
#endif

}
