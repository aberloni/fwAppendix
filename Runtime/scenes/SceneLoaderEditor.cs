using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace fwp.scenes
{
	public class SceneLoaderEditor
	{
#if UNITY_EDITOR
		static public void loadScene(string nm, OpenSceneMode mode = OpenSceneMode.Additive)
		{
			string path = HalperScene.getPathOfSceneInProject(nm);
			if (path.Length <= 0)
			{
				Debug.LogWarning($" no path for {nm} in build settings");
				return;
			}

			Debug.Log("(editor) OpenScene(" + path + ")");

			EditorSceneManager.OpenScene(path, mode);
		}

		static public void loadSceneByBuildSettingsPresence(string nm, bool forceAddToBuildSettings = false, OpenSceneMode mode = OpenSceneMode.Additive)
		{

			// check if in build settings
			if (!HalperScene.isSceneInBuildSettings(nm, true))
			{
				//  if NOT add to build settings

				if (forceAddToBuildSettings)
				{
					HalperScene.addSceneToBuildSettings(nm);
					Debug.Log($"added {nm} was re-added to build settings");
				}

			}

			string path = HalperScene.getBuildSettingsFullPathOfScene(nm);
			if (path.Length <= 0)
			{
				Debug.LogWarning($" no path for {nm} in build settings");
				return;
			}

			EditorSceneManager.OpenScene(path, mode);
		}
#endif
	}

}
