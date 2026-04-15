using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace fwp.buzz
{

	/// <summary>
	/// manager de la pile des choses a faire pendant le boot/setup du jeu
	/// tout ce qui se passe avant le runtime
	/// 
	/// /! THIS WILL LOCK INPUTS UNTIL lockers[] is empty
	/// 
	/// __hardlocking__
	/// each element indicates if it is hardlocking or not
	/// ie : to display loading screen
	/// 
	/// </summary>
	public class Buzz
	{
		static public Buzz instance;

		Dictionary<iBee, bool> lockers = new();

		/// <summary>
		/// react to sub/unsub events
		/// </summary>
		public Action<Buzz> onLockersCountChanged;

		/// <summary>
		/// need to display buzz state on screen
		/// aka : use buzz viewer
		/// </summary>
		virtual protected bool isVisible()
		{
			return Debug.isDebugBuild;
		}

		public Buzz()
		{
			instance = this;
			lockers.Clear();
		}

		public void sub(iBee bee, bool hard = true)
		{
			if (!Application.isPlaying) return;

			if (lockers.ContainsKey(bee))
			{
				lockers[bee] = hard;
			}
			else
			{
				lockers.Add(bee, hard);
				if (lockers.Count > 0 && isVisible()) BuzzViewer.fetch();
			}

			onLockersCountChanged?.Invoke(this);
		}

		public void unsub(iBee bee)
		{
			if (!Application.isPlaying) return;

			if (!lockers.ContainsKey(bee))
				return;

			lockers.Remove(bee);
			onLockersCountChanged?.Invoke(this);
		}

		public bool isLockingHard()
		{
			return lockers.Any(x => x.Value);
		}

		public bool isLocking()
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
				ret += "\n[" + l.Key.GetType() + "] " + l.Key.stringifyBeeState();
				ret += l.Value ? " +HARD" : " +SOFT";
			}

			return ret;
		}
	}

}