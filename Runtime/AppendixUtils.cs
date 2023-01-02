using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace fwp.appendix
{
	static public class AppendixUtils
	{

		static public T[] gcs<T>() where T : UnityEngine.Object
		{
			return GameObject.FindObjectsOfType<T>();
		}

		/// <summary>
		/// gc == getcomponent
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		static public T gc<T>() where T : UnityEngine.Object
		{
			return GameObject.FindObjectOfType<T>();
		}

		static public T gc<T>(string containtName) where T : UnityEngine.Object
		{
			if (containtName.Length <= 0) return gc<T>();

			T[] list = GameObject.FindObjectsOfType<T>();
			for (int i = 0; i < list.Length; i++)
			{
				if (list[i].name.Contains(containtName)) return list[i];
			}
			return null;
		}


		/// <summary>
		/// TROP PA OPTI
		/// </summary>
		static public T[] getCandidates<T>()
		{
			GameObject[] all = GameObject.FindObjectsOfType<GameObject>();
			List<T> tmp = new List<T>();
			for (int i = 0; i < all.Length; i++)
			{
				T inst = all[i].GetComponent<T>();
				if (inst != null)
				{
					tmp.Add(inst);
				}
			}
			return tmp.ToArray();
		}




		/// <summary>
		/// EXTRACTS FROM fwp.halpers:HalperScene
		/// 2023-01-02
		/// </summary>


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

#if UNITY_EDITOR

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

		static public string getSceneAssetFullPath(string sceneName)
		{
			string fullName = getBuildSettingsSceneFullName(sceneName);

			string[] paths = getAssetScenesPaths();

			for (int i = 0; i < paths.Length; i++)
			{
				if (!paths[i].Contains(".unity")) continue;

				if (paths[i].Contains(sceneName)) return paths[i];
			}

			return string.Empty;
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


		static public string[] getAssetScenesPaths()
		{
			string[] paths = AssetDatabase.FindAssets("t:Scene");

			if (paths.Length <= 0)
			{
				Debug.LogWarning("asking for scene but none ?");
			}

			//replace GUID by full path
			for (int i = 0; i < paths.Length; i++)
			{
				paths[i] = AssetDatabase.GUIDToAssetPath(paths[i]);
			}

			return paths;
		}

#endif

	}

}
