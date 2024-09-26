using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace fwp.industries
{

    public interface IFactory
    {
        public void refresh(); // refresh content of this factory
        public bool hasCandidates(); // factory as any elements ?
        public void recycleAll(); // force a recycling on all active elements

        //public void injectObject(iFactoryObject instance);

        public iFactoryObject extract(string uid);
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