using System;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.industries
{
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    using Object = UnityEngine.Object;

    /// <summary>
    /// Assets/[factory path]/[sub type] ?(.prefab)
    /// </summary>
    abstract public class FactoryAddr<Type> : FactoryBase<Type> where Type : class, iFactoryObject
    {
        Dictionary<string, AddrPair> pairs = new Dictionary<string, AddrPair>();

        public class AddrPair
        {
            public AsyncOperationHandle<GameObject> asyncOp;
            public GameObject addrBlob;

            public void free()
            {
                Addressables.Release(addrBlob);
            }
        }

        virtual protected string solvePath(string addrKey)
        {
            return "Assets/" + addrKey + ".prefab";
        }

        void fetchAddr(string path, Action<GameObject> onPresence = null)
        {
            IndustriesVerbosity.sLog("<b>fetch</b>@" + path);

            AddrPair pair = new AddrPair();

            pair.asyncOp = Addressables.LoadAssetAsync<GameObject>(path);

            pairs.Add(path, pair);

            pair.asyncOp.Completed += (obj) =>
            {
                switch (obj.Status)
                {
                    case AsyncOperationStatus.Succeeded:

                        IndustriesVerbosity.sLog("<b>fetch</b>&success @" + path);

                        pair.addrBlob = obj.Result;

                        onPresence?.Invoke(pair.addrBlob);

                        break;
                    case AsyncOperationStatus.Failed:
                    default:
                        Debug.LogError("<b>fetch</b>&failure " + obj.Status + " @ " + path);
                        break;
                }

            };

        }

        void getBlob(string path, Action<GameObject> onPresence)
        {
            if (pairs.ContainsKey(path))
            {
                onPresence(pairs[path].addrBlob);
            }
            else
            {
                fetchAddr(path, onPresence);
            }
        }

        protected override Object instantiate(string path)
        {
            throw new NotImplementedException("<NOT ASYNC> can't instantiate instant using addr");
        }

        protected override void instantiate(string path, Action<Object> onPresence)
        {
            getBlob(solvePath(path), (blob) =>
            {
                onPresence(
                    GameObject.Instantiate(blob));
            });
        }
    }

}
