using UnityEditor;
using UnityEngine;

namespace fwp.platforms.editor
{
    using fwp.platforms;

    static public class PlatformMenuItems
    {
        [MenuItem("Platform/-log current-")]
        static void miLog() => Debug.Log($"[PlatformForcer] current = {PlatformForcer.Current}");

        [MenuItem("Platform/switch1", false)]
        static void miSwitch() => PlatformForcer.force(PlatformForcer.Platform.switch_1);

        [MenuItem("Platform/switch1", true)]
        static bool miSwitchValidate()
        {
            UnityEditor.Menu.SetChecked("Platform/switch1", PlatformForcer.Is(PlatformForcer.Platform.switch_1));
            return true;
        }
    }

}
