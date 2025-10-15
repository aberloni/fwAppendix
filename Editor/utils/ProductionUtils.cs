using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Diagnostics;
using System.IO;

namespace fwp.prod
{
	static public class ProductionUtils
	{
		/*
		[UnityEditor.MenuItem("Production/(open) readme")]
		static public void OpenReadme()
		{
			OpenLocalMarkdown("readme");
		}

		[UnityEditor.MenuItem("Production/(open) roadmap")]
		static public void OpenRoadmap()
		{
			OpenLocalMarkdown("roadmap");
		}
		*/

		static public void OpenLocalMarkdown(string file)
		{
			var fileExt = file + ".md";

			var path = Application.dataPath.Replace("/", "\\");
			path = path.Substring(0, path.LastIndexOf("\\"));
			path = Path.Combine(path, fileExt);

			//UnityEngine.Debug.Log(path);
			//path = Path.GetFullPath(path);

			if (!File.Exists(path))
			{
				if (UnityEditor.EditorUtility.DisplayDialog(
					"Missing " + fileExt + " ?", "Do you want to create : " + fileExt,
					"Yes", "No"))
				{
					File.Create(path);
				}
				else
				{
					//UnityEngine.Debug.LogWarning("no readme found ? " + path);
					return;
				}
			}

			UnityEngine.Debug.Log(" > " + path);
			Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
		}
	}

}
