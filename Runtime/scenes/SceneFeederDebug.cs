using UnityEngine;

namespace fwp.scenes
{

    public class SceneFeederDebug : SceneLoaderFeederBase
    {
        
        [Header("#debug || isDebugBuild")]
        public FeederData scenes;

        protected override void solveNames()
        {
            
            if (isDebug())
            {
                Debug.LogWarning($"feeder:<b>{GetType()}</b>");
                addFeederData(scenes);
            }

        }
        
        virtual protected bool isDebug()
        {
#if debug
            return true;
#else
            return Debug.isDebugBuild;
#endif
        }

    }
}