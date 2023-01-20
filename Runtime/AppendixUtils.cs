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

	}

}
