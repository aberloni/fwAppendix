namespace fwp.industries
{
    using facebook;

    public class IndusReferenceMgr : Facebook
    {
        static public bool verbose => IndustriesVerbosity.verbose;

        static IndusReferenceMgr _instance;
        static public IndusReferenceMgr Instance
        {
            get
            {
                if (_instance == null) _instance = new IndusReferenceMgr();
                return _instance;
            }
        }

        virtual protected bool mustClearOnStartup() => false;

		[UnityEngine.RuntimeInitializeOnLoadMethod]
		static void runtime() 
        {
            if(Instance.mustClearOnStartup())
            {
                Instance.Clear();
            }
        }
        
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