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
        public bool hasAnyCandidates();

        /// <summary>
        /// force a recycling on all active elements
        /// make all element inactive
        /// </summary>
        public void recycleAll();

        //public void injectObject(iFactoryObject instance);

        /// <summary>
        /// READ ONLY
        /// get from pools actives[] OR recycled[]
        /// </summary>
        public iFactoryObject browse(string uid);

        /// <summary>
        /// READ from recycled[]
        /// CREATE if missing
        /// </summary>
        public iFactoryObject extract(string uid);

        /// <summary>
        /// recycle OR create
        /// some factory will need some async behavior (ie : addressables)
        /// </summary>
        public void extractAsync(string uid, System.Action<iFactoryObject> onPresence);

        /// <summary>
        /// bool is to inject either in inactives or actives list of factory
        /// if active : will also add to matching facebook
        /// return : successful injection
        /// </summary>
        public bool inject(iFactoryObject candidate);

        /// <summary>
        /// keep factory posted
        /// called by candidate when object is recycled by context
        /// </summary>
        public bool recycle(iFactoryObject candidate);

        /// <summary>
        /// keep factory posted
        /// called by candidate when it s destroyed
        /// </summary>
        public bool destroy(iFactoryObject candidate);

#if UNITY_EDITOR
        /// <summary>
        /// DEBUG ONLY
        /// creates a copy in a list
        /// </summary>
        public List<iFactoryObject> edGetActives();
        public List<iFactoryObject> edGetInactives();
#endif

    }

}
