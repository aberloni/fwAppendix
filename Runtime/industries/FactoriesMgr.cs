using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace fwp.industries
{
	/// <summary>
	/// manager of all FACTORIES
	/// </summary>
	static public class FactoriesMgr
	{
		static private List<iFactory> factos = new List<iFactory>(); // FactoryBase<iFactoryObject>

		/// <summary>
		/// for debug only
		/// </summary>
		static public List<iFactory> getAllFactories() => factos;

		static public void recycleEverything()
		{
			Debug.Log("Facto  :  recycle all");
			foreach (var facto in factos)
			{
				facto.recycleAll();
			}
		}

		/// <summary>
		/// ie => seek : BrainScrapling
		/// </summary>
		static public Facto getFactoryOf<Facto>()	where Facto : iFactory
		{
			//Type typ = typeof(Facto);

			//already exists ?
			for (int i = 0; i < factos.Count; i++)
			{
				Facto f = (Facto)factos[i];
				if (f != null)
					return f;

				//if (factos[i].GetType() == typ) return (T)factos[i];
			}

			return create<Facto>();
		}

		/// <summary>
		/// create the factory instance
		/// </summary>
		static private Facto create<Facto>() where Facto : iFactory
		{
			//https://stackoverflow.com/questions/731452/create-instance-of-generic-type-whose-constructor-requires-a-parameter

			Facto fb;

			object instance = Activator.CreateInstance<Facto>();
			Debug.Assert(instance != null);

			fb = (Facto)instance;
			Debug.Assert(fb != null, $"implem for {typeof(Facto)} , check typo ?");

			Debug.Log($" creating new factory for {typeof(Facto)} , total x{factos.Count}");

			//FactoryBase fb = (FactoryBase)Activator.CreateInstance(typeof(FactoryBase), new object[] { tarType });
			factos.Add(fb);

			return fb;
		}

		static public void inject(iFactory facto)
        {
			factos.Add(facto);
        }
	}

}
