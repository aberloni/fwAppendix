
namespace fwp.platforms
{
    static public class PlatformForcer
    {
        public enum Platform
        {
            none,

            //desktop
            window,
            osx,
            linux,

            // mobiles
            android,
            ios,

            // consoles
            switch_1,
            switch_2,
            xbox_series,
            playstation_5,
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Platform/-reset-")]
        static public void miReset() => force(Platform.none);

        [UnityEditor.MenuItem("Platform/switch1")]
        static void miSwitch() => force(Platform.switch_1);
#endif

        static public void force(Platform p) => Current = p;

        static public bool IsSwitch
        {
            get
            {
                switch (Current)
                {
                    case Platform.switch_1:
                    case Platform.switch_2:
                        return true;
                }
                return false;
            }
        }

        static public bool IsConsole
        {
            get
            {
                if (IsSwitch) return true;
                return false;
            }
        }


        static Platform _current = Platform.none;
        static bool _loaded = false;

        /// <summary>
        /// was forced to something
        /// </summary>
        static public bool IsSet => Current != Platform.none;

        static public bool Is(Platform p) => Current == p;

        /// <summary>
        /// what platform forced to ?
        /// </summary>
        static public Platform Current
        {
            get
            {
                if (!_loaded) load();
                return _current;
            }
            private set
            {
                _current = value;
                save();
            }
        }

        static string uniqKey
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.PlayerSettings.productGUID.ToString();
#else
                return Application.dataPath.GetHashCode();
#endif
            }
        }

        static string kPrefKey = "fwp.platform.forced_" + uniqKey;

        static void load()
        {
#if UNITY_EDITOR
            _current = (Platform)UnityEditor.EditorPrefs.GetInt(kPrefKey, (int)Platform.none);
#else
            // in builds
            _current = Platform.none;
#endif
            _loaded = true;
        }

        static void save()
        {
#if UNITY_EDITOR
            UnityEditor.EditorPrefs.SetInt(kPrefKey, (int)_current);
#endif
        }

    }

}
