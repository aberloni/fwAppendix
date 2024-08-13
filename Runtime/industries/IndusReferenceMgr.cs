namespace fwp.industries
{
    using facebook;

    public class IndusReferenceMgr : Facebook
    {
        static public bool verbose => IndustriesVerbosity.verbose;

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