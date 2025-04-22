using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace fwp.industries
{

    public interface IFactory
    {
        public bool isTargetType(System.Type type);

        /// <summary>
        /// refresh content of this factory
        /// </summary>
        public void refresh();

        /// <summary>
        /// factory as any elements ?
        /// has something in pool
        /// </summary>
        public bool hasCandidates();

        /// <summary>
        /// force a recycling on all active elements
        /// make all element inactive
        /// </summary>
        public void recycleAll();

        //public void injectObject(iFactoryObject instance);

        /// <summary>
        /// create OR recycle inactive
        /// </summary>
        public iFactoryObject extract(string uid);

        /// <summary>
        /// some factory will need some async behavior (ie : addressables)
        /// </summary>
        public void extractAsync(string uid, System.Action<iFactoryObject> onPresence);

#if UNITY_EDITOR
        /// <summary>
        /// DEBUG ONLY
        /// creates a copy in a list
        /// </summary>
        public List<iFactoryObject> getActives();
        public List<iFactoryObject> getInactives();
#endif
    }

}
