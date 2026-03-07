using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

namespace fwp.buzz
{

	/// <summary>
	/// manager de la pile des choses a faire pendant le boot/setup du jeu
	/// tout ce qui se passe avant le runtime
	/// 
	/// /! THIS WILL LOCK INPUTS UNTIL lockers[] is empty
	/// 
	/// </summary>
	public class Buzz
	{
		[RuntimeInitializeOnLoadMethod]
		static void runtime()
		{
			new Buzz();
		}

		static public Buzz instance;

		List<iBee> lockers = new List<iBee>();

		public Action<bool> onLocked;

		public Buzz()
		{
			instance = this;

			lockers.Clear();
		}

		public void sub(iBee bee)
		{
			if (!Application.isPlaying) return;

			if (lockers.Contains(bee))
				return;

			lockers.Add(bee);
			if (lockers.Count > 0) BuzzViewer.fetch();
			onLocked?.Invoke(lockers.Count > 0);
		}

		public void unsub(iBee bee)
		{
			if (!Application.isPlaying) return;

			if (!lockers.Contains(bee))
				return;

			lockers.Remove(bee);
			onLocked?.Invoke(lockers.Count > 0);
		}

		public bool isBusy()
		{
			return lockers.Count > 0;
		}

		public object getToken()
		{
			return new object();
		}

		public string stringify()
		{
			string ret = "lockers x" + lockers.Count;
			foreach (var l in lockers)
			{
				ret += "\n[" + l.GetType() + "] " + l.stringifyBeeState();
			}

			return ret;
		}
	}

	/// <summary>
	/// buzz:bee
	/// locking if subbed
	/// </summary>
	public interface iBee
	{
		/// <summary>
		/// showed on screen while this bee is subbed
		/// </summary>
		public string stringifyBeeState();
	}

	/// <summary>
	/// + feedback why locking
	/// </summary>
	public interface iBeeDyn : iBee
	{
		/// <summary>
		/// method to modify bee state
		/// </summary>
		public void setBuzz(string msg);

		public void clearBuzz(); // reset bee state
	}

	public class BuzzViewer : MonoBehaviour
	{
		static public BuzzViewer fetch()
		{
			// no VIEW if buzz in not active
			if (Buzz.instance == null) return null;

			var bv = FindAnyObjectByType<BuzzViewer>();
			if (bv != null) return bv;
			return new GameObject("~buzz").AddComponent<BuzzViewer>();
		}

		private void Awake()
		{
			DontDestroyOnLoad(this);
		}

		private void LateUpdate()
		{
			if (!Buzz.instance.isBusy()) // itself
				GameObject.Destroy(gameObject);
		}

		const float w = 300f;
		private void OnGUI()
		{
			GUI.Label(new Rect(Screen.width - w, 30, w, 800), Buzz.instance.stringify());
		}
	}
}