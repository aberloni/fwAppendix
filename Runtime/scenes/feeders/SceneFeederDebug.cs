using UnityEngine;

namespace fwp.scenes.feeder
{

    public class SceneFeederDebug : SceneLoaderFeederBase
    {

        [Header("isDebugBuild")]
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
            return Debug.isDebugBuild;
        }

    }
}