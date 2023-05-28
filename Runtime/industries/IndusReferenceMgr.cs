using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using Object = UnityEngine.Object;

namespace fwp.industries
{
    /// <summary>
    /// FACEBOOK wrapper
    /// need to specify compatible types
    /// </summary>
    public class IndusReferenceMgr : ReferenceFacebook<iIndusReference>
    {
        static IndusReferenceMgr _instance;
        static public IndusReferenceMgr instance
        {
            get
            {
                if (_instance == null) _instance = new IndusReferenceMgr();
                return _instance;
            }
        }

    }

    public interface iIndusReference
    { }
}