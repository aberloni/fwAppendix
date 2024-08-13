using System;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.industries
{
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    using Object = UnityEngine.Object;

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

        /// <summary>
        ///
        /// </summary>
        void fetchAddr(string path, Action<GameObject> onPresence = null)
        {

            path = "Assets/" + path + ".prefab";

            /*
            if (pairs.ContainsKey(path))
            {
                onPresence.Invoke(pairs[path].addrBlob);
                return;
            }
            */

            AddrPair pair = new AddrPair();

            pair.asyncOp = Addressables.LoadAssetAsync<GameObject>(path);

            pairs.Add(path, pair);
            
            pair.asyncOp.Completed += (obj) =>
            {
                switch (obj.Status)
                {
                    case AsyncOperationStatus.Failed: Debug.LogError("fail @ " + path); break;
                    case AsyncOperationStatus.Succeeded:
                        pair.addrBlob = obj.Result;

                        onPresence?.Invoke(pair.addrBlob);

                        break;
                }

            };

        }

        GameObject getBlob(string path, Action<GameObject> onPresence = null)
        {
            if (pairs.ContainsKey(path))
            {
                return pairs[path].addrBlob;
            }

            fetchAddr(path, onPresence);

            return null; // use presence callback instead
        }

        protected override Object instantiate(string path)
        {
            throw new NotImplementedException("can't instantiate instant using addr");
        }

        protected override void instantiate(string path, Action<UnityEngine.Object> onPresence)
        {
            getBlob(path, (blob) =>
            {
                onPresence(
                    GameObject.Instantiate(blob));
            });
        }
    }

}
