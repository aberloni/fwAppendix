using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using System.Linq;

namespace fwp.industries
{
	using System;
	using System.Threading;
	using Object = UnityEngine.Object;

	/// <summary>
	/// wrapper object to make a factory for a specific type
	/// - actives[] is a copy from the content in facebook
	/// - pool[] is all element (active or not)
	/// </summary>
	abstract public class FactoryBase<FaceType> : IFactory where FaceType : class, iFactoryObject
	{
		/// <summary>
		/// ReadOnly wrapper around list in facebook
		/// </summary>
		ReadOnlyCollection<FaceType> actives = null;

		/// <summary>
		/// all objects currently available for recycling
		/// </summary>
		List<FaceType> pool = null;

		System.Type _factoryTargetType;

		public FactoryBase()
		{
			_factoryTargetType = getFactoryTargetType();

			// get handle to RO list
			actives = IndusReferenceMgr.instance.GetGroup<FaceType>();
			pool = new();

			if (!Application.isPlaying) refresh(false);
		}

		/// <summary>
		/// what kind of object will be created by this factory
		/// </summary>
		protected System.Type getFactoryTargetType() => typeof(FaceType);

		public bool isTargetType(Type type) => getFactoryTargetType() == type;

		public void refresh(bool includeInactives)
		{
			log($"refresh(inactives ? {includeInactives})");

			pool.Clear();

			Object[] presents = (Object[])fwp.appendix.AppendixUtils.gcts(typeof(FaceType), includeInactives);
			for (int i = 0; i < presents.Length; i++)
			{
				inject(presents[i] as FaceType);
			}

			log("refresh:after x{actives.Count}");
		}

		//abstract public System.Type getChildrenType();

		public bool hasAnyCandidates() => pool.Count > 0;
		public bool hasAnyActiveCandidates() => actives.Count > 0;
		public bool hasAmountCandidates(int countCheck) => pool.Count >= countCheck;

		/// <summary>
		/// cannot implem this
		/// must use facebook RO collects
		/// 
		/// SHK to facebook content
		/// just transfert list
		/// </summary>
		//public ReadOnlyCollection<FaceType> getActives() => actives;

		/// <summary>
		/// get any reference from active list
		/// null if empty list
		/// </summary>
		public FaceType getRandomActive()
		{
			if (actives == null) return null;
			if (actives.Count <= 0) return null;
			return actives[UnityEngine.Random.Range(0, actives.Count)];
		}

		/// <summary>
		/// the next one in the last based on given instance
		/// </summary>
		public FaceType getNextActive(FaceType curr)
		{
			int idx = actives.IndexOf(curr);
			if (idx > -1)
			{
				if (idx + 1 < actives.Count) return actives[idx + 1];
				return actives[0]; // loop
			}

			Debug.LogError(curr + " is not in factory ?");

			return null;
		}

		/// <summary>
		/// path = factory folder (if any) / subtype
		/// this will load object blob AND instantiate
		/// </summary>
		abstract protected void instantiateAsync(string path, Action<UnityEngine.Object> onPresence);

		/// <summary>
		/// immediate load, no async
		/// </summary>
		abstract protected Object instantiate(string path);

		virtual protected string solvePath(string subType)
		{
			string folder = getObjectPath();
			if (!string.IsNullOrEmpty(folder)) subType = folder + "/" + subType;
			return subType;
		}

		/// <summary>
		/// async creation
		/// </summary>
		protected void createAsync(string subType, Action<FaceType> onPresence = null)
		{
			string path = solvePath(subType);
			instantiateAsync(path, (instance) =>
			{
				onPresence.Invoke(
					solveNew(instance));
			});
		}

		/// <summary>
		/// instant creation
		/// generate a new element in the pool
		/// </summary>
		protected FaceType create(string subType)
		{
			string path = getObjectPath() + "/" + subType;

			log("no " + subType + " available (x" + pool.Count + ") : new");

			var instance = instantiate(path);
			if (instance == null) return null;

			return solveNew(instance);
		}

		FaceType solveNew(Object copy)
		{
			if (copy == null) return null;
			GameObject go = copy as GameObject;

			//Debug.Log("newly created object " + go.name, go);

			FaceType candidate = go.GetComponent<FaceType>();
			Debug.Assert(candidate != null, $"could not retrieve comp<{typeof(FaceType)}> on gObject:{go} ?? generated object is not factory compatible", go);

			pool.Add(candidate);

			return candidate;
		}

		/// <summary>
		/// path within Resources/ where to find object to load
		/// this is only the path, without the name of the object
		/// object name/uid is located on interface
		/// 
		/// ie : "Bullets" -> Resources/Bullets
		/// </summary>
		abstract protected string getObjectPath();

		//public iFactoryObject extractObject(string subType) => (iFactoryObject)extract(subType);

		public iFactoryObject browse(string uid)
		{
			return extractFromPool(uid);
		}

		/// <summary>
		/// recycle OR create
		/// same as fetch, but fetch return generic type
		/// </summary>
		public iFactoryObject extract(string uid)
		{
			iFactoryObject ret = extractFromRecycled(uid);

			// create if missing
			if (ret == null) ret = create(uid);

			// flag as active
			if (ret != null) inject(ret);

			return ret;
		}

		/// <summary>
		/// use fetch instead
		/// same as fetch, but return interface type
		/// </summary>
		public void extractAsync(string uid, Action<iFactoryObject> onPresence)
		{
			iFactoryObject ret = extractFromRecycled(uid);

			if (ret == null)
			{
				createAsync(uid, (createdInstance) =>
				{
					inject(createdInstance);
					onPresence?.Invoke(createdInstance);
				});

			}
			else
			{
				inject(ret);
				onPresence?.Invoke(ret);
			}

		}

		/*
        public T extract<T>(string subType, Action<FaceType> onPresence)
        {
            FaceType icand = extract(subType, onPresence);
            Component com = icand as Component;
            return com.GetComponent<T>();
        }
        */

		bool isCandidateActive(FaceType candidate)
		{
			return actives.Contains(candidate);
		}

		FaceType extractFromActives(string uid)
		{
			foreach (var c in actives)
			{
				if (c.GetCandidateName() == uid) return c;
			}
			return null;
		}
		FaceType extractFromRecycled(string uid)
		{
			foreach (var c in pool)
			{
				if (isCandidateActive(c)) continue;
				if (c.GetCandidateName() == uid) return c;
			}
			return null;
		}
		FaceType extractFromPool(string uid)
		{
			foreach (var c in pool)
			{
				if (c.GetCandidateName() == uid) return c;
			}

			return null;
		}

		/// <summary>
		/// shk
		/// </summary>
		public bool recycle(iFactoryObject candid)
		{
			if (candid as FaceType == null)
			{
				Debug.LogWarning($"trying to recycle {candid} but not of facto FaceType<{typeof(FaceType)}>");
				return false;
			}

			return recycle(candid as FaceType);
		}

		/// <summary>
		/// indiquer a la factory qu'un objet a changé d'état de recyclage
		/// true : object was removed from listing
		/// </summary>
		public bool recycle(FaceType candid)
		{
			if (!pool.Contains(candid))
			{
				log("recycle:   candidate is not part of pool, can't recycle");
				return false;
			}

			bool dirty = false;

			// remove from active pool
			if (actives.Contains(candid))
			{
				IndusReferenceMgr.instance.Remove(candid);
				candid.OnRecycled();
				dirty = true;
			}

			// REMOVE PARENTING

			MonoBehaviour comp = candid as MonoBehaviour;

			// edge case where recycling is called when destroying the object
			if (!IsNullOrDestroyed(comp))
			{
				if (comp.transform != null)
				{
					comp.transform.SetParent(null);
				}
			}

			if (dirty) log("recycled:   " + candid + " :: " + stringifyOneLiner());

			return dirty;
		}

		public bool inject(iFactoryObject candid) => inject(candid as FaceType);

		/// <summary>
		/// to flag as used by facto
		/// generaly called when object is created (by facto or instance of a scene)
		/// - auto if created by factory
		/// - also can be used for pre-existing object in scene to flag part of facto
		/// </summary>
		public bool inject(FaceType candid)
		{
			Debug.Assert(candid != null, "candid to inject is null ?");

			bool dirty = false; // something changed ?

			if (!pool.Contains(candid))
			{
				pool.Add(candid);
				dirty = true;
			}

			if (!actives.Contains(candid))
			{
				// also try to add to facebook
				MonoBehaviour cmp = candid as MonoBehaviour;
				if (cmp != null) cmp.enabled = true;
				IndusReferenceMgr.instance.Register<FaceType>(candid);

				dirty = true;
			}

			if (dirty) log("inject :: " + candid + " :: " + stringifyOneLiner());

			return dirty;
		}

		public bool destroy(iFactoryObject candid) => destroy(candid as FaceType);

		/// <summary>
		/// called by a destroyed object
		/// </summary>
		public bool destroy(FaceType candid)
		{
			// remove from facebook
			IndusReferenceMgr.instance.Remove(candid);

			pool.Remove(candid);

			return true;
		}

		public void recycleAll()
		{
			log("recycleAll()");

			// copy, to avoid index shifting
			List<FaceType> cands = new(actives);

			// use INTERNAL to avoid inf loops

			for (int i = 0; i < cands.Count; i++)
			{
				var candid = cands[i];

				recycle(candid);
			}

			Debug.Assert(actives.Count <= 0);
		}

		string getStamp() => "<color=#3333aa>" + GetType() + "|" + _factoryTargetType + "</color>";

		public string stringifyOneLiner() => $"↑{actives.Count}/ x{pool.Count}";

		void log(string content, object target = null)
		{

#if UNITY_EDITOR || industries
			IndustriesVerbosity.sLog(getStamp() + content, target as Object);
#endif
		}

		/// <summary>
		/// https://forum.unity.com/threads/how-to-check-if-a-gameobject-is-being-destroyed.1030849/
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static bool IsNullOrDestroyed(System.Object obj)
		{
			if (object.ReferenceEquals(obj, null)) return true;

			if (obj is UnityEngine.Object) return (obj as UnityEngine.Object) == null;

			return false;
		}

#if UNITY_EDITOR

		/// <summary>
		/// DEBUG
		/// </summary>
		public List<iFactoryObject> edGetActives()
		{
			return new(actives);
		}

		/// <summary>
		/// DEBUG
		/// </summary>
		public List<iFactoryObject> edGetInactives()
		{
			List<iFactoryObject> ret = new(pool);
			foreach (var c in actives)
			{
				ret.Remove(c);
			}
			return ret;
		}

#endif

	}

}
