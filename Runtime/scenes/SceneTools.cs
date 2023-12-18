using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace fwp.scenes
{
    static public class SceneTools
    {

        static public bool isDebugScene()
        {
            if (isSceneOfType("test-")) return true;
            if (isSceneOfType("debug-")) return true;
            return false;
        }

        static public bool isTestScene()
        {
            if (isSceneOfType("test-")) return true;
            return false;
        }

        static public string getLevelName() { return SceneManager.GetActiveScene().name; }
        static public bool isSceneOfType(string nm) { return getLevelName().StartsWith(nm); }
        static public bool isSceneOfName(string nm) { return getLevelName().Contains(nm); }

        /* remove everything with a * at the start of the name */
        static public void removeGuides()
        {
            GameObject[] objs = GameObject.FindObjectsOfType<GameObject>();
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].name[0] == '*') GameObject.Destroy(objs[i].gameObject);
            }
        }

        static public string removePathBeforeFile(string path)
        {
            return path.Substring(path.LastIndexOf("/") + 1);
        }

        static public string removeUnityExt(string path)
        {
            if (path.EndsWith(".unity")) path = path.Substring(0, path.IndexOf(".unity"));
            return path;
        }

		/// <summary>
		/// returns path relative to unity project (starts with assets/)
		/// remove sys to remove part of the path outside of unity
		/// </summary>
		static public List<string> getScenesPathsOfCategory(string folderContains, bool removeExt = false)
		{
			string[] scenes = new string[0];

#if UNITY_EDITOR

			if (!Application.isPlaying)
			{
				// !runtime : asset database
				scenes = getProjectAssetScenesPaths();
			}

#endif

			if(scenes.Length <= 0)
            {
				// @runtime / @build, use build settings list
				scenes = getAllBuildSettingsScenes(false);
			}

			List<string> output = new List<string>();

			if (scenes.Length <= 0)
            {
                Debug.LogWarning("no scenes ?");
                return output;
            }

            for (int i = 0; i < scenes.Length; i++)
            {
                string pathLower = scenes[i].ToLower();

				//if (pathLower.Contains("/3rd")) continue;

				// remove _layer after scene prefix
				// sim-a_lol -> sim-a
				//pathLower = pathLower.Split("_")[0];

                if (!pathLower.Contains(folderContains.ToLower())) continue;

                string path = scenes[i];
                if (removeExt) path = removeUnityExt(path);
                output.Add(path);
            }

            //Debug.Log($"found x{regionScenes.Count} regions");

            return output;
        }

#if UNITY_EDITOR
		/// <summary>
		/// only returns scene names
		/// </summary>
		static public List<string> getScenesNamesOfCategory(string cat)
        {
            List<string> paths = getScenesPathsOfCategory(cat, true);
            List<string> output = new List<string>();

            foreach(string path in paths)
            {
                // remove path
                string scName = removePathBeforeFile(path);

                // remove ext, jic
                //scName = removeUnityExt(scName);

                output.Add(scName);
            }

            //Debug.Log($"found x{regionScenes.Count} regions");

            return output;
        }
#endif

        static public bool isRuntimeSceneLoaded(string sceneName)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
            return scene.isLoaded;
        }

#if UNITY_EDITOR
        static public bool isEditorSceneLoaded(string sceneName)
        {
			var scene = getEditorSceneLoaded(sceneName);
			return scene != null;
        }

		static public UnityEngine.SceneManagement.Scene? getEditorSceneLoaded(string sceneName)
        {
			var scene = EditorSceneManager.GetSceneByName(sceneName);
			if(scene.IsValid() && scene.isLoaded)
            {
				return scene;
            }
			return null;
		}

#endif


		static public bool checkIfCanBeLoaded(string sceneName)
		{
			string[] all = getAllBuildSettingsScenes(true);
			for (int i = 0; i < all.Length; i++)
			{
				if (all[i].Contains(sceneName)) return true;
			}
			return false;
		}

		static public bool isSceneOpened(string sceneName)
		{
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				Scene sc = SceneManager.GetSceneAt(i);
				if (sc.name == sceneName) return true;
			}
			return false;
		}

		static public string[] getAllBuildSettingsScenes(bool removePath)
		{
			List<string> paths = new List<string>();

			//Debug.Log(SceneManager.sceneCountInBuildSettings);

			for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
			{
				string path = SceneUtility.GetScenePathByBuildIndex(i);

				if (removePath)
				{
					int slashIndex = path.LastIndexOf('/');

					if (slashIndex >= 0)
					{
						path = path.Substring(slashIndex + 1);
					}

					path = path.Remove(path.LastIndexOf(".unity"));

				}

				paths.Add(path);
			}

			return paths.ToArray();
		}


#if UNITY_EDITOR

		static public string getSceneAssetFullPath(string partName)
		{
			// in project t:Scene
			string[] paths = getProjectAssetScenesPaths();

			for (int i = 0; i < paths.Length; i++)
			{
				if (!paths[i].Contains(".unity")) continue;
				if (paths[i].Contains(partName)) return paths[i];
			}

			return string.Empty;
		}

		static public string getBuildSettingsFullPathOfScene(string partName)
		{
			string fullName = getBuildSettingsSceneFullName(partName);
			string[] paths = getAllBuildSettingsScenes(false);
			for (int i = 0; i < paths.Length; i++)
			{
				if (paths[i].Contains(fullName))
				{
					return paths[i];
				}
			}

			return string.Empty;
		}


		static public void addSceneToBuildSettings(string sceneName)
		{
			if (isSceneInBuildSettings(sceneName, true)) return;

			string assetPath = getSceneAssetFullPath(sceneName);

			//string fullName = getBuildSettingsSceneFullName(sceneName);

			List<EditorBuildSettingsScene> all = new List<EditorBuildSettingsScene>();
			all.AddRange(EditorBuildSettings.scenes);

			//string path = getBuildSettingsFullPathOfScene(sceneName);

			EditorBuildSettingsScene addScene = new EditorBuildSettingsScene(assetPath, true);
			all.Add(addScene);

			EditorBuildSettings.scenes = all.ToArray();
		}


		static public string getPathOfSceneInProject(string sceneName)
		{
			string[] guids = AssetDatabase.FindAssets("t:Scene");

			for (int i = 0; i < guids.Length; i++)
			{
				// Assets/Modules/module-a-b.unity
				string path = AssetDatabase.GUIDToAssetPath(guids[i]);

				string pathSceneName = path.Substring(0, path.LastIndexOf("."));
				pathSceneName = pathSceneName.Substring(pathSceneName.LastIndexOf("/") + 1);

				// module-a-b
				//Debug.Log(pathSceneName);

				if (pathSceneName == sceneName) return path;
			}
			return string.Empty;
		}


		static public bool isSceneInBuildSettings(string partName, bool hardCheck = false)
		{

			string nm = getBuildSettingsSceneFullName(partName);
			if (nm.Length < 0) return false;

			if (hardCheck) return nm == partName;
			return true;
		}

		static public string getBuildSettingsSceneFullName(string partName)
		{
			if (partName.EndsWith(".unity")) partName = partName.Substring(0, partName.IndexOf(".unity"));

			string[] all = getAllBuildSettingsScenes(true); // no path
			for (int i = 0; i < all.Length; i++)
			{
				if (all[i].Contains(partName))
				{
					return all[i];
				}
			}
			return string.Empty;
		}

		static string[] _scene_paths;

		/// <summary>
		/// fetch all scene present in database
		/// this should return all scene in projet
		/// </summary>
		static public string[] getProjectAssetScenesPaths()
		{
			if (_scene_paths != null) return _scene_paths;

			_scene_paths = AssetDatabase.FindAssets("t:Scene");

			if (_scene_paths.Length <= 0)
			{
				Debug.LogWarning("asking for scene but none ?");
			}

			//replace GUID by full path
			for (int i = 0; i < _scene_paths.Length; i++)
			{
				_scene_paths[i] = AssetDatabase.GUIDToAssetPath(_scene_paths[i]);
			}

			return _scene_paths;
		}

#endif

	}

}
