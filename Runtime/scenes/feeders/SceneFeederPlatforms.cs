using UnityEngine;

namespace fwp.scenes.feeder
{

    /// <summary>
    /// a feeder that will only work on specific platforms
    /// </summary>
    public class SceneFeederPlatforms : SceneLoaderFeederBase
    {
        [Header("STEAMWORKS")]
        [SerializeField]
        SceneLoaderFeeder.FeederData feedSteam;

        [Header("UNITY_SWITCH")]
        [SerializeField]
        SceneLoaderFeeder.FeederData feedSwitch;

        virtual protected bool isSteam()
        {
#if STEAMWORKS
            return true;
#else
            return false;
#endif
        }

        virtual protected bool isSwitch()
        {
#if UNITY_SWITCH
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