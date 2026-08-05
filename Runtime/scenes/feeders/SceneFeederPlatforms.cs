using UnityEngine;

namespace fwp.scenes.feeder
{

    /// <summary>
    /// a feeder that will only work on specific platforms
    /// +AUTOFEED
    /// </summary>
    public class SceneFeederPlatforms : SceneLoaderFeederBase
    {
        [System.Serializable]
        public class FeederSteam : SceneLoaderFeeder.FeederData
        {
            public FeederSteam()
            {
                category = "steam";
            }
        }

        [Header("STEAMWORKS")]
        [SerializeField]
        FeederSteam feedSteam;

        [System.Serializable]
        public class FeederSwitch : SceneLoaderFeeder.FeederData
        {
            public FeederSwitch()
            {
                category = "switch";
            }
        }

        [Header("UNITY_SWITCH")]
        [SerializeField]
        FeederSwitch feedSwitch;

        virtual protected bool isSteam()
        {
#if STEAMWORKS || STEAM
            return true;
#else
            return false;
#endif
        }

        virtual protected bool isSwitch()
        {
#if UNITY_SWITCH || SWITCH
            return true;
#else
            return false;
#endif
        }

        protected override void solveNames()
        {

            if (isSteam())
            {
                Debug.Log($"feeder:<b>STEAM</b>", this);
                addFeederData(feedSteam);
            }

            if (isSwitch())
            {
                Debug.Log($"feeder:<b>SWITCH</b>", this);
                addFeederData(feedSwitch);
            }

        }

    }

}