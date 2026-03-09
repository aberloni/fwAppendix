using UnityEngine;

namespace fwp.scenes.feeder
{

    /// <summary>
    /// feeder +switch platform
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
                Debug.LogWarning("FEEDER.STEAM-SDK");
                addWithPrefix("resource-", "steam");
            }

            if (isSwitch())
            {
                Debug.LogWarning("FEEDER.SWITCH");
                addWithPrefix("resource-", "switch");
                // addNoPrefix(switch_only);
            }

        }
        
    }

}