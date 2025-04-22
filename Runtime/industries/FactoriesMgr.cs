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

        static public IFactory getFactoryOf(System.Type type)
        {
            // already exists ?
            foreach (var f in factos)
            {
                Debug.Assert(f != null, "an item in factos[] is null ??");
                try
                {
                    // need to compare type
                    // can't cast if not matching
                    if (f.isTargetType(type))
                    {
                        return f;
                    }
                }
                catch
                {
                    Debug.LogError("facto cast :: can't cast " + type.ToString());
                }
            }

            return create(type);
        }

        /// <summary>
        /// get a factory by its subtype
        /// </summary>
        static public T getFactoryOf<T>() where T : IFactory
        {
            var facto = getFactoryOf(typeof(T));
            return (T)facto;
        }

        static private IFactory create(Type type)
        {
            //https://stackoverflow.com/questions/731452/create-instance-of-generic-type-whose-constructor-requires-a-parameter
            object instance = Activator.CreateInstance(type);

            IFactory fb = instance as IFactory;
            Debug.Assert(fb != null, $"implem for {type.ToString()} , check typo ?");

            factos.Add(fb);
            if (IndusReferenceMgr.verbose) Debug.Log($"Facto:   created new factory <b>{type.ToString()}</b> , total x{factos.Count}");

            return fb;
        }

        /// <summary>
        /// create the factory instance
        /// </summary>
        static private T create<T>() where T : IFactory
        {
            return (T)create(typeof(T));
        }

        static public void inject(IFactory facto)
        {
            factos.Add(facto);
        }
    }

}
