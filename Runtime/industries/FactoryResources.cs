using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace fwp.industries
{
    abstract public class FactoryResources<Type> : FactoryBase<Type> where Type : class, iFactoryObject
    {
        Dictionary<string, Object> buffer = new Dictionary<string, Object>();

        Object checkPresence(string path)
        {
            Object blob;

            if (!buffer.ContainsKey(path))
            {
                blob = Resources.Load(path);
                buffer.Add(path, blob);
            }
            else
            {
                blob = buffer[path];
            }

            return blob;
        }

        protected override Object instantiate(string path)
        {
            Object blob = checkPresence(path);

            if (blob == null)
            {
                Debug.LogWarning("/! <color=red>null object</color> path@" + path);
                return null;
            }

            var copy = GameObject.Instantiate(blob);

            Debug.Log("created copy : " + copy, copy);

            return copy;
        }

        protected override void instantiate(string path, Action<Object> onPresence)
        {
            //Object blob = checkPresence(path);
            onPresence(instantiate(path));
        }
    }
}