using UnityEditor;
using UnityEngine;

namespace fwp.platforms.editor
{
    using fwp.platforms;

    static public class PlatformMenuItems
    {
        const string mi_path = "Window/Platform/";
        const string mi_switch1 = mi_path + "force:switch1";

        [MenuItem(mi_switch1, false)] static void miSwitch1() => toggle(PlatformForcer.Platform.switch_1);
        [MenuItem(mi_switch1, true)] static bool miSwitch1Validate() => check(mi_switch1, PlatformForcer.Platform.switch_1);

        // one pair per platform...

        [MenuItem(mi_path + "-reset-", false, 100)] static void miReset() => PlatformForcer.force(PlatformForcer.Platform.none);

        /// <summary>
        /// click on active platform : back to none
        /// </summary>
        static void toggle(PlatformForcer.Platform p)
            => PlatformForcer.force(PlatformForcer.Is(p) ? PlatformForcer.Platform.none : p);

        /// <summary>
        /// validation : refresh checkmark, always enabled
        /// </summary>
        static bool check(string path, PlatformForcer.Platform p)
        {
            Menu.SetChecked(path, PlatformForcer.Is(p));
            return true;
        }
    }
}
