using UnityEngine;
using fwp.scenes;

namespace fwp.scenes
{

	/// <summary>
	/// (scene wrapper) for specific scene name
	/// -> to simplify sorting
	/// </summary>
	public struct SceneProfilTarget
	{
		public struct PatternOrder
		{
			public string suffix;
			public int order;
		}

		string sceneName;
		public string Name => sceneName;

		/// <summary>
		/// priority in loading order
		/// </summary>
		int order;
		public int Order => order;

		/// <summary>
		/// stage of loading, to separate when this profil is loaded
		/// 0 = normal load
		/// 1 = later
		/// </summary>
		uint delayOrder;
		public uint Delay => delayOrder;

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
			order = 0;
			delayOrder = 0;
			
			setOrder(ord);
		}

		public void setOrder(int ord)
		{
			order = ord;
		}

		public bool Contains(string filter)
		{
			return sceneName.ToLower().Contains(filter);
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

}