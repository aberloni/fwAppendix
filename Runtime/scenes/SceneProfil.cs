using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace fwp.scenes
{
	/// <summary>
	/// CONTEXT_SCENE{_LAYER}
	/// associer autour d'une UID un ensemble de scene
	/// multi layering scenes
	/// </summary>
	public class SceneProfil
	{
		/// <summary>
		/// externaly activated when needed
		/// </summary>
		static public bool verbose = false;

		string _category; // debug
		public string Category => _category;

		string _profilDefaultScenePath; // path to first scene found
		public string PingScenePath => _profilDefaultScenePath;

		string _profilPath; // Assets/path/to/profil/scenes
		public string ParentPath => _profilPath;

		// path : [context]_scene_layer
		// base_sub(_layer)
		// base_sub
		string context; // context OR context_scene

		bool _dirty = false;

		public string label => context;

		//these are only scene names (no ext, no path)

		/// <summary>
		/// all scenes linked to this profil
		/// </summary>
		public List<SceneProfilTarget> layers;

		/// <summary>
		/// other contextual scenes needed for this profil
		/// that will be unloaded with profil
		/// </summary>
		public List<string> deps;

		/// <summary>
		/// scenes that won't be unload on profil unload
		/// </summary>
		public List<string> statics;

		List<SceneAssoc> _assocs_buff;

		/// <summary>
		/// has found anything
		/// </summary>
		public bool HasLayers
		{
			get
			{
				if (layers == null) return false;
				return layers.Count > 0;
			}
		}

		public bool IsValid
		{
			get
			{
				if (string.IsNullOrEmpty(_profilPath)) return false;
				if (layers.Count <= 0) return false;
				return true;
			}
		}

		public bool matchPath(string path)
		{
			return SceneTools.removePathBeforeFile(path) == _category;
		}

		/// <summary>
		/// any of the layer within has filter contains
		/// </summary>
		public bool matchFilter(string filter)
		{
			if (string.IsNullOrEmpty(filter)) return true;
			if (!HasLayers) return false;

			filter = filter.ToLower();

			if (!label.ToLower().Contains(filter))
			{
				return false;
			}

			foreach (var l in layers)
			{
				if (l.Contains(filter))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// categoryUid is uniq PATH to scenes
		/// OR simply name of a context
		/// 
		/// ingame, want to load a scene
		/// force add to build settings is not available in builds
		/// </summary>
		public SceneProfil(string categoryUid)
		{
			// from unity's resources ??
			if (categoryUid.Contains("SceneManagement"))
			{
				Debug.LogError("invalid uid : " + categoryUid);
				return;
			}

			// only keep context, remove path
			// path must be deduce by context
			_category = SceneTools.removePathBeforeFile(categoryUid);
			if (verbose) Debug.Log(" + SceneProfil <b>" + _category + "</b>");

			//Debug.Assert(categoryUid.Split("_").Length < 2, categoryUid + " cannot be partial : CONTEXT_SCENE_LAYER");

			context = extractContextFromPath(_category);

			if (string.IsNullOrEmpty(context))
			{
				Debug.LogError("/! profil : input = " + _category + " not compat");
				return;
			}

			if (verbose) Debug.Log(" + Context      <b>" + context + "</b>");

			//if (verbose) Debug.Log("solved context : " + context);
			//Debug.Log(categoryUid + " ? " + solvedCategoryUid);

			solveLayers(categoryUid, getPaths());

			// nothing here
			// but context might want to add stuff
			solveDeps();
			solveStatics();

			if (!IsValid)
			{
				Debug.LogWarning("profil path must not be null : " + _category);
				Debug.LogWarning("maybe context is not added to build settings");
			}
		}

		/// <summary>
		/// all path to base scene of this profil
		/// </summary>
		virtual protected string[] getPaths()
		{
			// this might return null
			// @runtime : if scenes are not present in build settings
			// must give root name of category (no layer)
			var paths = filterAllPaths(true);

			if (paths == null) return null;

			// remove non-compat
			paths = filterPaths(paths);

			return paths.ToArray();
		}

		/// <summary>
		/// solve all base scenes of this profil
		/// </summary>
		virtual protected void solveLayers(string categoryUid, string[] paths)
		{
			if (layers == null) layers = new();

			if (paths == null) return;

			// solve layers & deps paths
			// adds deps

			layers.Clear();

			foreach (var p in paths)
			{
				SceneProfilTarget spt = new SceneProfilTarget(p, 0);
				layers.Add(spt);
			}

			if (verbose) Debug.Log(categoryUid + " : layers x " + layers.Count + " (parsed path x " + paths.Length + ")");
		}

		public void sortByPattern(string[] suffixes, int[] orders)
		{
			if (suffixes == null)
			{
				if (verbose) Debug.Log(Category + " :     no pattern");
				return;
			}

			Debug.Assert(suffixes.Length == orders.Length);

			if (verbose) Debug.Log(Category + " order by pattern x" + suffixes.Length);

			// sort by pattern
			List<SceneProfilTarget> output = new List<SceneProfilTarget>();
			for (int i = 0; i < suffixes.Length; i++)
			{
				string suff = suffixes[i];
				int order = orders[i];

				for (int j = 0; j < layers.Count; j++)
				{
					if (layers[j].IsPriority(suff))
					{
						layers[j].setOrder(order);
						output.Add(layers[j]);
						layers.RemoveAt(j);
					}
				}
			}

			// +re-inject ignored by pattern
			foreach (var l in layers)
			{
				output.Add(l);
			}

			layers = output; // replace by ordered
		}

		public void refresh()
		{
			if (Application.isPlaying)
				return;

			if (_assocs_buff == null)
				_dirty = true;

			if (_dirty)
			{
				fetchAssocs(true);
			}

			_dirty = false;
		}

		public void setDirty()
		{
			_dirty = true;
		}

		void solveProfilPath(string refPath)
		{
			Debug.Assert(!string.IsNullOrEmpty(refPath), "no ref path given ?");
			Debug.Assert(refPath.IndexOf("/") > 0, "path has no '/' ?");

			// store path to scene (for ping)
			_profilDefaultScenePath = refPath;

			// keep any of the path as reference
			// to gatekeep others
			// remove scene name, keep only path
			_profilPath = refPath.Substring(0, refPath.LastIndexOf("/"));

			if (verbose) Debug.Log(" + path     <b>" + _profilPath + "</b>");
		}

		/// <summary>
		/// extract all suited scenes from assetdatabase
		/// </summary>
		List<string> filterAllPaths(bool removeExt = false)
		{
			// gets ALL paths containing this cUID
			// checks if context is contained in scenes path
			var paths = getPaths(context, removeExt);
			if (paths.Count <= 0)
			{
				Debug.LogWarning($"given base context : <b>{context}</b> => empty paths[] (length = 0)");
				Debug.LogWarning("target context was <color=red>not added to build settings</color> ?");
				return null;
			}

			solveProfilPath(paths[0]);

			// filter paths
			if (verbose) Debug.Log("filter paths (context:" + context + ") from x" + paths.Count);

			for (int i = 0; i < paths.Count; i++)
			{
				// same path = keep
				bool toRemove = !checkSamePath(paths[i]);

				if (!toRemove)
				{
					if (verbose) Debug.Log("#" + i + " : " + paths[i]);
				}
				else
				{
					//Debug.Log("ignored: " + paths[i]);
					paths.RemoveAt(i);
					i--;
				}
			}

			return paths;
		}

		public bool match(SceneProfil sp)
		{
			return sp.label == label;
		}

		/// <summary>
		/// returns path to profil
		/// </summary>
		List<string> filterPaths(List<string> paths)
		{
			//if (verbose) Debug.Log("profil: setup(" + context + ") paths x" + paths.Count);

			Debug.Assert(paths.Count > 0, "paths empty ?");

			// clean paths
			for (int i = 0; i < paths.Count; i++)
			{
				paths[i] = SceneTools.removePathBeforeFile(paths[i]);
			}

			return paths;
		}

		/// <summary>
		/// force add = force adding all target scene into build settings
		/// </summary>
		virtual protected List<string> getPaths(string uid, bool removeExt = false)
		{
			var paths = SceneTools.getScenesPathsOfCategory(uid, removeExt);
			//var paths = SceneTools.getScenesPathsOfCategory(uid);
			return paths;
		}

		/// <summary>
		/// ce path est compat avec ce profil ?
		/// return : true if same path
		/// </summary>
		bool checkSamePath(string path)
		{
			// both this profil AND given path must share same path
			string copy = path.Substring(0, path.LastIndexOf("/"));

			return copy == _profilPath;
		}

		/// <summary>
		/// trying to extract uid aka "context{_scene}" from path
		/// everything but suffix
		/// 
		/// beeing able to solve uids differently
		/// scene name must always be the last or the n-1
		/// like : scene-name_layer => scene-name
		/// 
		/// base_sub_layer => base_sub
		/// 
		/// </summary>
		static public string extractContextFromPath(string path)
		{
			string ret = SceneTools.removePathBeforeFile(path);
			string[] split = ret.Split('_');

			// scene-name
			if (split.Length <= 0) return ret;

			// base_sub_layer
			if (split.Length > 2)
			{
				ret = ret.Substring(0, ret.LastIndexOf("_"));
			}

			return ret;
		}

		/// <summary>
		/// pile de toutes les sc�nes qui seront a charger au runtime
		/// </summary>
		virtual public void solveDeps()
		{
			if (deps == null) deps = new List<string>();
			deps.Clear();
		}

		virtual public void solveStatics()
		{
			if (statics == null) statics = new List<string>();
			statics.Clear();
		}

		public bool isLoaded()
		{
			if (layers.Count <= 0) return false;
			foreach (var l in layers)
			{
				if (!l.IsLoaded)
					return false;
			}
			return true;
		}

#if UNITY_EDITOR

		void forceAddToBuildSettings()
		{
			List<EditorBuildSettingsScene> tmp = new List<EditorBuildSettingsScene>();

			// keep existing
			if (EditorBuildSettings.scenes != null)
			{
				if (EditorBuildSettings.scenes.Length > 0)
					tmp.AddRange(EditorBuildSettings.scenes);
			}

			var scenes = filterAllPaths(false); // gather linked scenes

			if (scenes.Count <= 0)
			{
				if (verbose)
					Debug.LogWarning("no scenes returned after filtering ?");

				return;
			}

			foreach (string path in scenes)
			{
				// no duplicates
				if (tmp.Exists(x => x.path == path))
				{
					if (verbose)
						Debug.LogWarning("duplicate, skipping");

					continue;
				}

				// /! path NEEDS extension
				tmp.Add(new EditorBuildSettingsScene(path, true));
			}

			if (tmp.Count <= 0)
			{
				if (verbose)
					Debug.LogWarning("nothing to add to build settings ?");

				return;
			}

			//assign
			EditorBuildSettings.scenes = tmp.ToArray();

			if (verbose)
			{
				Debug.Log("was (re)added to build settings x" + tmp.Count);

				for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
				{
					Debug.Log("#" + i + " => " + EditorBuildSettings.scenes[i].path);
				}

				Debug.Log("total build settings scenes x" + EditorBuildSettings.scenes.Length);
			}

		}

		/// <summary>
		/// replace context = remove all other scenes
		/// </summary>
		public void editorLoad(bool replaceContext, bool forceAddBuildSettings = false)
		{
			// first check that scenes are added to build settings ?
			if (forceAddBuildSettings) forceAddToBuildSettings();

			if (verbose)
				Debug.Log($"SceneProfil:editorLoad <b>{label}</b> ; layers x{layers.Count} & deps x{deps.Count}");

			// first : load base scene NON ADDITIVE to replace full context
			// additive check : might wanna replace context
			if (layers.Count > 0)
			{
				UnityEditor.SceneManagement.OpenSceneMode mode = UnityEditor.SceneManagement.OpenSceneMode.Single;
				if (!replaceContext) mode = UnityEditor.SceneManagement.OpenSceneMode.Additive;

				string baseScene = layers[0].Name;
				if (verbose) Debug.Log($"SceneProfil:loading base scene {baseScene}");
				SceneLoaderEditor.loadScene(baseScene, mode);
			}

			List<string> toLoads = new List<string>();

			foreach (var l in layers)
			{
				toLoads.Add(l.Name);
			}

			toLoads.AddRange(deps);
			toLoads.AddRange(statics);

			// load all
			// layers[0] is empty ?
			for (int i = 0; i < toLoads.Count; i++)
			{
				if (verbose) Debug.Log($"SceneProfil:loading layer:{toLoads[i]}");
				SceneLoaderEditor.loadScene(toLoads[i]); // additive
			}
		}

		public void editorUnload()
		{
			//solveDeps();

			if (verbose) Debug.Log($"SceneProfil:unload");

			for (int i = 0; i < layers.Count; i++)
			{
				layers[i].editorUnload();
			}

			for (int i = 0; i < deps.Count; i++)
			{
				SceneLoaderEditor.unloadScene(deps[i]);
			}

			// NOT STATICS : statics are meant to stay loaded

			//var sc = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByName(layers[0]);
			//UnityEditor.SceneManagement.EditorSceneManager.CloseScene(sc, true);
		}
#endif

		/// <summary>
		/// create a virtual delay after loadings layers & deps
		/// </summary>
		virtual protected float getDebugLoadDelay() => 0f;

		public void buildLoad(Action<SceneProfil> onLoadedCompleted)
		{
			//solveDeps();

			if (!IsValid)
			{
				Debug.LogWarning("INVALID : can't load : " + context);
				onLoadedCompleted?.Invoke(this);
				return;
			}

			if (verbose) Debug.Log(getStamp() + " builload");

			loadStatics(() =>
			{
				if (verbose) Debug.Log(getStamp() + "   statics.loaded");

				loadDeps(() =>
				{
					if (verbose) Debug.Log(getStamp() + "   deps.loaded");

					loadLayers(() =>
					{
						if (verbose) Debug.Log(getStamp() + "   layers.loaded");

						//Scene? parentScene = extractMainScene(false);
						onLoadedCompleted?.Invoke(this);
					});
				});

			});

		}

		void loadDependencies(string[] scenes, Action onCompletion)
		{

			if (scenes.Length <= 0)
			{
				//Debug.LogWarning(getStamp() + " deps array is empty ?");
				onCompletion.Invoke();
				return;
			}

			if (verbose)
			{
				Debug.Log(getStamp() + " load some dependencies x" + scenes.Length);
				for (int i = 0; i < scenes.Length; i++) Debug.Log(getStamp() + " scene:" + scenes[i]);
			}

			float delay = 0f;

#if UNITY_EDITOR
			delay = getDebugLoadDelay();
#endif

			SceneLoader.loadScenes(scenes, (scs) =>
			{
				onCompletion?.Invoke();
			}, delay);
		}

		void loadStatics(Action onCompletion) => loadDependencies(statics.ToArray(), onCompletion);
		void loadDeps(Action onCompletion) => loadDependencies(deps.ToArray(), onCompletion);

		void loadLayers(Action onCompletion)
		{
			if (layers.Count <= 0)
			{
				//Debug.LogWarning(getStamp() + " layers array is empty ?");
				onCompletion.Invoke();
				return;
			}

			if (verbose)
			{
				Debug.Log(getStamp() + " loading layers x" + layers.Count);
				for (int i = 0; i < layers.Count; i++) Debug.Log(getStamp() + " layer:" + layers[i]);
			}

			if (_assocs_buff == null) _assocs_buff = new List<SceneAssoc>();

			SceneLoader.loadScenes(getLayersScenesNames(), (SceneAssoc[] scs) =>
				{
					if (scs.Length <= 0)
					{
						Debug.LogError("no scenes returned ?");
						for (int i = 0; i < layers.Count; i++)
						{
							Debug.Log("  " + layers[i]);
						}
					}

					_assocs_buff.AddRange(scs);

					//Scene main = extractMainScene();
					//Debug.Assert(main.IsValid(), getStamp()+" extracted scene : " + main + " is not valid");

					onCompletion.Invoke();
				});
		}

		string[] getLayersScenesNames()
		{
			string[] ret = new string[layers.Count];
			for (int i = 0; i < layers.Count; i++)
			{
				ret[i] = layers[i].Name;
			}
			return ret;
		}

		/// <summary>
		/// 
		/// </summary>
		public void buildUnload(System.Action onUnloadCompleted)
		{

			if (verbose)
				Debug.Log(getStamp() + " build unload : <b>" + label + "</b>");

			if (layers == null)
			{
				if (verbose)
					Debug.Log(getStamp() + " null layers");

				onUnloadCompleted?.Invoke();
				return;
			}

			if (layers.Count <= 0)
			{
				if (verbose)
					Debug.Log(getStamp() + " empty layers");

				onUnloadCompleted?.Invoke();
				return;
			}

			SceneLoader.unloadScenes(getLayersScenesNames(), onUnloadCompleted);
		}

		List<SceneAssoc> fetchAssocs(bool force)
		{
			if (_assocs_buff == null)
				_assocs_buff = new List<SceneAssoc>();

			if (_assocs_buff.Count <= 0 || force)
			{
				_assocs_buff.Clear();

				_assocs_buff.AddRange(SceneAssoc.solveScenesAssocs(getLayersScenesNames()));
				_assocs_buff.AddRange(SceneAssoc.solveScenesAssocs(deps.ToArray()));

				//if (verbose) Debug.Log("assocs x" + _assocs_buff.Count);
			}

			return _assocs_buff;
		}

		public GameObject extractRoot(string sceneName, string rootName)
		{
			Scene? sc = extractScene(sceneName);

			if (sc == null)
				return null;

			foreach (var root in sc.Value.GetRootGameObjects())
			{
				if (root.name.Contains(rootName))
					return root;
			}

			return null;
		}

		public Scene? extractScene(string nm)
		{
			refresh();

			if (_assocs_buff == null)
				return null;

			if (_assocs_buff.Count <= 0)
			{
				return null;
			}

			foreach (var assoc in _assocs_buff)
			{
				if (assoc.handle.name.Contains(nm))
				{
					return assoc.handle;
				}
			}

			return null;
		}

		public Scene? extractMainScene()
		{
			refresh();

			if (_assocs_buff == null)
				return null;

			if (_assocs_buff.Count <= 0)
			{
				return null;
			}

			if (_assocs_buff[0].isLoaded())
			{
				return _assocs_buff[0].handle;
			}

			return null;
		}

		virtual public string editor_getButtonName()
		{
			string ret = label;
			if (layers.Count > 0) ret += " x" + layers.Count;
			if (deps.Count > 0) ret += " +d" + deps.Count;
			if (statics.Count > 0) ret += " +s" + statics.Count;
			return ret;
		}

		virtual public string stringify()
		{
			string output = label;
			if (!string.IsNullOrEmpty(_profilPath)) output += "     profil path : " + _profilPath;
			if (layers != null) output += "lyr[" + layers.Count + "] & deps[" + deps.Count + "]";
			return output;
		}

		string getStamp()
		{
			return "{SceneProfil} " + stringify();
		}



	}


}

public struct SceneAssoc
{
	public string path;
	public Scene handle;
}