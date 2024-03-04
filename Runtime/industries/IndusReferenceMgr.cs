using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using Object = UnityEngine.Object;

namespace fwp.industries
{
    using facebook;

    public class IndusReferenceMgr : Facebook
    {
        static public bool verbose;

        static IndusReferenceMgr _instance;
        static public IndusReferenceMgr instance
        {
            get
            {
                if (_instance == null) _instance = new IndusReferenceMgr();
                return _instance;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Window/Industries/(verbose) factory")]
        static public void toggleVerbose()
        {
            IndusReferenceMgr.verbose = !IndusReferenceMgr.verbose;
            Debug.LogWarning("toggling verbose for factories : " + IndusReferenceMgr.verbose);
        }
#endif

    }

    /*
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
    */

}