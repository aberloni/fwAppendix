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
        static private List<IFactory> factos = new List<IFactory>(); // FactoryBase<iFactoryObject>

        /// <summary>
        /// for debug only
        /// gives copy
        /// </summary>
        static public IFactory[] getAllFactories() => factos.ToArray();

        static public void recycleEverything()
        {
            Debug.Log("Facto  :  recycle all");
            foreach (var facto in factos)
            {
                facto.recycleAll();
            }
        }

        /// <summary>
        /// checks for explicit factory type
        /// </summary>
        static public T getFactory<T>() where T : IFactory
        {
            foreach (var f in factos)
            {
                try
                {
                    if (f.GetType() == typeof(T))
                    {
                        return (T)f;
                    }
                }
                catch
                {
                    Debug.LogError("facto cast :: can't cast " + typeof(T).ToString());
                }
            }
            return create<T>();
        }

        /// <summary>
        /// check if type is candidate type of factory
        /// instead of checking factory type
        /// </summary>
        static public IFactory getFactoryOfCandidateType(Type candidateType)
        {
            // already exists ?
            foreach (var f in factos)
            {
                Debug.Assert(f != null, "an item in factos[] is null ??");
                try
                {
                    // need to compare type
                    // can't cast if not matching
                    if (f.isTargetType(candidateType))
                    {
                        return f;
                    }
                }
                catch
                {
                    Debug.LogError("facto cast :: can't cast " + candidateType.ToString());
                }
            }

            return create(candidateType);
        }

        static private T create<T>() where T : IFactory
        {
            //https://stackoverflow.com/questions/731452/create-instance-of-generic-type-whose-constructor-requires-a-parameter
            object instance = Activator.CreateInstance<T>();

            T fb = (T)instance;
            Debug.Assert(fb != null, $"implem for {typeof(T)} , check typo ?");

            factos.Add(fb);
            if (IndusReferenceMgr.verbose) Debug.Log($"Facto:   created new factory <b>{typeof(T)}</b> , total x{factos.Count}");

            return fb;
        }

        /// <summary>
        /// create a factory based on sub type
        /// FactoryBase<GivenType>
        /// </summary>
        static private IFactory create(Type subtype)
        {
            Type ft = typeof(FactoryBase<>).MakeGenericType(subtype);

            //https://stackoverflow.com/questions/731452/create-instance-of-generic-type-whose-constructor-requires-a-parameter
            object instance = Activator.CreateInstance(ft);

            IFactory fb = instance as IFactory;
            Debug.Assert(fb != null, $"implem for sub<{subtype.ToString()}< , check typo ?");

            factos.Add(fb);
            if (IndusReferenceMgr.verbose) Debug.Log($"Facto:   created new factory <b>Factory<{subtype.ToString()}></b> , total x{factos.Count}");

            return fb;
        }

        static public void inject(IFactory facto)
        {
            factos.Add(facto);
        }
    }

}
