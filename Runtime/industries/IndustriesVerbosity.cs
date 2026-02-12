using UnityEngine;

static public class IndustriesVerbosity
{
    const string _id = "Industries";
    public const string menu_verbose_path = "Window/" + _id + "/verbose";
    const string _int_verbose = "fwp." + _id + ".verbosity";

#if UNITY_EDITOR
    [UnityEditor.MenuItem(menu_verbose_path)]
    static void ToggleVerbose() => Verbose = !Verbose;
#endif

    public enum VerbosityLevel
    {
        none,
        events,
        deep,
    }

    /// <summary>
    /// shk
    /// </summary>
    static public bool Verbose
    {
        get
        {
            return VerboseLevel > VerbosityLevel.none;
        }
        set
        {
            if (value) VerboseLevel = VerbosityLevel.events;
            else VerboseLevel = VerbosityLevel.none;
        }
    }

    static public VerbosityLevel VerboseLevel
    {
        get
        {
#if UNITY_EDITOR
            return (VerbosityLevel)UnityEditor.EditorPrefs.GetInt(_int_verbose, 0);
#else
            // OFF in builds
            return VerbosityLevel.none;
#endif
        }
        set
        {
#if UNITY_EDITOR
            UnityEditor.EditorPrefs.SetInt(_int_verbose, (int)value);
            Debug.LogWarning(_int_verbose + " : verbosity : " + VerboseLevel);
            UnityEditor.Menu.SetChecked(menu_verbose_path, value > 0); // not none
#endif
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Window/Appendix/" + _id + "/verbosity:check")]
    static void miScreensVerboseCheck() => Debug.Log(_int_verbose + ":" + VerboseLevel + "?" + Verbose);

    [UnityEditor.MenuItem("Window/Appendix/" + _id + "/verbosity:off")]
    static void miScreensVerboseNone() => VerboseLevel = VerbosityLevel.none;

    [UnityEditor.MenuItem("Window/Appendix/" + _id + "/verbosity:events")]
    static void miScreensVerboseEvents() => VerboseLevel = VerbosityLevel.events;

    [UnityEditor.MenuItem("Window/Appendix/" + _id + "/verbosity:deep")]
    static void miScreensVerboseDeep() => VerboseLevel = VerbosityLevel.deep;
#endif

    static public void sLog(string msg, object target = null)
    {
        if (Verbose)
        {
            Debug.Log("{" + _id + "} " + msg, target as Object);
        }
    }

    static public void swLog(string msg, object target = null)
    {
        if (Verbose)
        {
            Debug.LogWarning("/! {" + _id + "} " + msg, target as Object);
        }
    }
}
