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
        /// find object of type
        /// (DOES NOT INCLUDE DEACTIVATED OBJECTS)
        /// </summary>
        public void refresh(bool includeInactives);

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
        /// bool is to inject either in inactives or actives list of factory
        /// if active : will also add to matching facebook
        /// return : successful injection
        /// </summary>
        public bool inject(iFactoryObject candidate, bool isActive);

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
