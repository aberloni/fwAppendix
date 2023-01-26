using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace fwp.industries
{
	/// <summary>
	/// manager of all FACTORIES
	/// </summary>
	static public class IndusMgr
	{
		static private List<FactoryBase> factos = new List<FactoryBase>();

		/// <summary>
		/// for debug only
		/// </summary>
		static public FactoryBase[] getAllFactories() => factos.ToArray();

		static public void recycleEverything()
		{
			Debug.Log("Facto  :  recycle all");
			foreach (var facto in factos)
			{
				facto.recycleAll();
			}
		}

		static public List<FactoryBase> getSerializableFactories()
		{
			List<FactoryBase> output = new List<FactoryBase>();

			if (factos.Count <= 0)
			{
				Debug.LogWarning("no facto present ?");
				return output;
			}

			for (int i = 0; i < factos.Count; i++)
			{
                if (!factos[i].isFactoSaveLinked()) continue;
                output.Add(factos[i]);
			}

			//if (factos.Count <= 0) Debug.LogWarning("no facto present ?");

			return output;
		}

		/// <summary>
		/// ie => seek : BrainScrapling
		/// </summary>
		static public FactoryBase getFactory(Type seekFactoryType)
		{
			Debug.Assert(seekFactoryType != null, "given type is null ?");
			//Type factoTyp = typeof(seekFactoryType);

			FactoryBase output;

			//already exists ?
			for (int i = 0; i < factos.Count; i++)
			{
				if (factos[i].GetType() == seekFactoryType)
				{
					return factos[i];
				}
			}

			//Debug.Log(" (" + factos.Count + ") creating new factory for " + seekFactoryType);
			//FactoryBase<TinyObject> fb = (FactoryBase<TinyObject>)Activator.CreateInstance(seekFactoryType);
			//factos.Add(fb);

			//output = (FactoryBase)Activator.CreateInstance(typeof(FactoryBase), new object[] { seekFactoryType });
			output = create(seekFactoryType);

			return output;
		}

		static public void refreshFactory<T>() where T : FactoryBase
		{
			getFactory<T>().refresh();
		}

		/// <summary>
		/// T = FactoryX
		/// </summary>
		static public T getFactory<T>() where T : FactoryBase
		{
			return (T)getFactory(typeof(T));
		}

		/// <summary>
		/// create the factory instance
		/// </summary>
		static private FactoryBase create(Type tarType)
		{
			//https://stackoverflow.com/questions/731452/create-instance-of-generic-type-whose-constructor-requires-a-parameter

			FactoryBase fb = null;

			object instance = Activator.CreateInstance(tarType);
			Debug.Assert(instance != null);

			fb = (FactoryBase)instance;
			Debug.Assert(fb != null, $"implem for {tarType} , check typo ?");

			Debug.Log($" creating new factory for {tarType} , total x{factos.Count}");

			//FactoryBase fb = (FactoryBase)Activator.CreateInstance(typeof(FactoryBase), new object[] { tarType });
			factos.Add(fb);

			return fb;
		}

		static public void inject(FactoryBase facto)
        {
			factos.Add(facto);
        }
	}

}
