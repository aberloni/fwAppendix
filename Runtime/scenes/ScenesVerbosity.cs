using UnityEngine;

static public class ScenesVerbosity
{
    const string _id = "Scenes";
    const string _int_verbose = "fwp." + _id + ".verbosity";

    public enum VerbosityLevel
    {
        none,
        events,
        deep,
    }

    /// <summary>
    /// set : events level
    /// </summary>
    static public bool verbose
    {
        get
        {
            return verboseLevel > VerbosityLevel.none;
        }
        set
        {
            verboseLevel = VerbosityLevel.events;
        }
    }


    static public VerbosityLevel verboseLevel
    {
        get
        {
#if UNITY_EDITOR
            return (VerbosityLevel)UnityEditor.EditorPrefs.GetInt(_int_verbose, 0);
#endif
        }
        set
        {
#if UNITY_EDITOR
            UnityEditor.EditorPrefs.SetInt(_int_verbose, (int)value);
#endif

            Debug.LogWarning(_int_verbose + " : verbosity : " + verboseLevel);
        }
    }





#if UNITY_EDITOR
    [UnityEditor.MenuItem("Window/Appendix/" + _id + "/verbosity:check")]
    static void miScreensVerboseCheck() => Debug.Log(_int_verbose + ":" + verboseLevel + "?" + verbose);

    [UnityEditor.MenuItem("Window/Appendix/" + _id + "/verbosity:off")]
    static void miScreensVerboseNone() => verboseLevel = VerbosityLevel.none;

    [UnityEditor.MenuItem("Window/Appendix/" + _id + "/verbosity:events")]
    static void miScreensVerboseEvents() => verboseLevel = VerbosityLevel.events;

    [UnityEditor.MenuItem("Window/Appendix/" + _id + "/verbosity:deep")]
    static void miScreensVerboseDeep() => verboseLevel = VerbosityLevel.deep;
#endif

}
