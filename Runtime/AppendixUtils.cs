using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace fwp.appendix
{
	/// <summary>
	/// syntax shrink
	/// </summary>
	static public class qh
	{
		static public T[] gcs<T>() where T : UnityEngine.Object => AppendixUtils.gcs<T>();
        static public T gc<T>() where T : UnityEngine.Object => AppendixUtils.gc<T>();
    }

	static public class AppendixUtils
	{
		/// <summary>
		/// 
		/// </summary>
		static public T[] gcs<T>() where T : UnityEngine.Object
		{
#if UNITY_2023
            return GameObject.FindObjectsByType<T>(FindObjectsSortMode.None);
#else
			return GameObject.FindObjectsOfType<T>();
#endif
        }

        /// <summary>
        /// gc == getcomponent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        static public T gc<T>() where T : UnityEngine.Object
		{
#if UNITY_2023
            return GameObject.FindFirstObjectByType<T>();
#else
			return GameObject.FindObjectOfType<T>();
#endif
		}

		static public T gc<T>(string containtName) where T : UnityEngine.Object
		{
			if (containtName.Length <= 0) return gc<T>();

			T[] list = gcs<T>();
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
			GameObject[] all = gcs<GameObject>();
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

	}

}
