using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace fwp.localization
{
    /// <summary>
    /// Manager qui s'occupe de la loca au editor/runtime
    /// 
    /// https://developer.nintendo.com/group/development/g1kr9vj6/forums/english/-/gts_message_boards/thread/269684575#636486
    /// 
    /// pour les espaces insécables : Alt+0160 pour l'écrire dans excel mais \u00A0 dans TMPro.
    /// https://forum.unity.com/threads/why-there-is-no-setting-for-textmesh-pro-ugui-to-count-whitespace-at-the-end.676897/
    /// </summary>
    public class LocalizationManager
    {
        static protected LocalizationManager instance;

        /// <summary>
        /// must be created in context
        /// </summary>
        static public LocalizationManager get()
        {
            if (instance == null) new LocalizationManager();
            return instance;
        }

        //au runtime il  faut que les candidates s'inscrivent !
        static public List<iLanguageChangeReact> reacts = new List<iLanguageChangeReact>();

        public const string LANG_PREFIX = "lang";

        public const IsoLanguages languageFallback = IsoLanguages.en; // si la langue du system est pas supportée

        static public IsoLanguages[] allSupportedLanguages = new IsoLanguages[]
        {
            IsoLanguages.en, IsoLanguages.fr, IsoLanguages.de, IsoLanguages.es, IsoLanguages.it
        };

        /// <summary>
        /// subfolder within Resources/
        /// </summary>
        public const string folder_localization = "localization/";
        
        /// <summary>
        /// where all txt files is located in the project
        /// </summary>
        public const string path_resource_localization = "Resources/" + folder_localization;

        LocalizationFile[] lang_files;

        public LocalizationManager()
        {
            instance = this;

            //CultureInfo.GetCultureInfo(CultureTypes.NeutralCultures)

            IsoLanguages iso = getSavedIsoLanguage(); //au cas ou, set default (fr)

            if (Application.isPlaying) Debug.Log("~Language~ starting language is <b>" + iso + "</b>");

            loadFiles();

            //Debug.Log("loaded " + lang_files.Length + " files");

            //remonte les erreurs
            //checkIntegrity();
        }

        /// <summary>
        /// must implem a way to solve sheet labels
        /// can be extended with custom sheets
        /// </summary>
        virtual public LocaDataSheetsIdLabel getSheetLabels()
        {
            return null;
        }

        protected void loadFiles()
        {

            List<LocalizationFile> tmp = new List<LocalizationFile>();
            for (int i = 0; i < allSupportedLanguages.Length; i++)
            {
                LocalizationFile file = new LocalizationFile(allSupportedLanguages[i]);
                if (file != null && file.isLoaded()) tmp.Add(file);
            }
            lang_files = tmp.ToArray();

        }

        static public void applySavedLanguage() => applyLanguage(getSavedIsoLanguage());

        /// <summary>
        /// A apl quand on change la lang
        /// </summary>
        static public void applyLanguage(IsoLanguages newLang)
        {
            Debug.Log("<color=cyan>applyLanguage</color> to <b>" + newLang + "</b>!");

            IsoLanguages iso = getSavedIsoLanguage();

            if (!Application.isPlaying)
            {
                reacts.Clear();
                reacts.AddRange(fwp.appendix.AppendixUtils.getCandidates<iLanguageChangeReact>());
            }

            Debug.Log("applying new lang (" + iso + ") to x" + reacts.Count + " reacts");

            for (int i = 0; i < reacts.Count; i++)
            {
                reacts[i].onLanguageChange(iso.ToString());
            }

        }

        public LocalizationFile getFileByLang(string lang)
        {
            for (int i = 0; i < lang_files.Length; i++)
            {
                //debug, NOT runtime, to be sure content is updated
                if (!Application.isPlaying) lang_files[i].debugRefresh();

                if (lang_files[i].lang_name == lang)
                {
                    return lang_files[i];
                }
            }
            return null;
        }

        protected void checkIntegrity()
        {

            for (int i = 0; i < lang_files.Length; i++)
            {
                for (int j = 0; j < lang_files.Length; j++)
                {
                    if (lang_files[i].compare(lang_files[j]))
                    {
                        Debug.LogError("Issue comparing " + lang_files[i].lang_name + " VS " + lang_files[j].lang_name);
                    }
                }
            }

        }

        public LocalizationFile getCurrentLangFile()
        {
#if UNITY_EDITOR
            loadFiles();
#endif

            string lang = getSavedIsoLanguage().ToString();
            LocalizationFile file = getLangFileByLangLabel(lang);

            if (file == null)
            {
                Debug.LogWarning(" !!! <color=red>no file</color> for current lang : " + lang);
                Debug.LogWarning(" !!! <color=red>this needs to be fixed before release</color> !!! ");

                IsoLanguages iso = getSystemLanguageToIso();
                Debug.LogWarning(" DEBUG <b>force</b> switching lang to '" + iso + "'");
                setSavedLanguage(iso);

                file = getLangFileByLangLabel(lang);
            }

            Debug.Assert(file != null, "file  " + lang + " should be assigned at this point ...");

            return file;
        }

        protected LocalizationFile getLangFileByLangLabel(string langLabel)
        {
            for (int i = 0; i < lang_files.Length; i++)
            {
                if (lang_files[i].lang_name == langLabel) return lang_files[i];
            }
            Debug.LogWarning(" !!! <color=red>no file</color> for current lang : " + langLabel);
            return null;
        }

        /// <summary>
        /// tries a getContent
        /// if fails return default value (en ?)
        /// </summary>
        public string getContentSafe(string id, bool warning = false)
        {
            string output = getContent(id);
            if(output.Length <= 0)
            {
                output = getContent(id, languageFallback, warning);
            }

            if(output.Length <= 0)
            {
                Debug.LogError("could not safe get content : " + id);
                return string.Empty;
            }

            return output;
        }

        public string getContent(string id, bool warning = false)
        {
            IsoLanguages lang = getSavedIsoLanguage();

            LocalizationFile file = instance.getFileByLang(lang.ToString());
            Debug.Assert(file != null, "no file found for language : " + lang);

            return file.getContentById(id, warning);
        }

        public string getContent(string id, IsoLanguages filterLang, bool warning = true)
        {
            LocalizationFile file = instance.getFileByLang(filterLang.ToString());
            Debug.Assert(file != null, "no file found for language : " + filterLang);

            return file.getContentById(id, warning);
        }



        /// <summary>
        /// on SWITCH platform there is a specific setup for this to work
        /// https://developer.nintendo.com/group/development/g1kr9vj6/forums/english/-/gts_message_boards/thread/269684575#636486
        /// none defined language in player settings won't work
        /// </summary>
        static public SystemLanguage getSystemLanguage() => Application.systemLanguage;

        static public int getLanguageIndex() => getLanguageIndex(getSavedIsoLanguage());

        static public int getLanguageIndex(IsoLanguages lang)
        {
            for (int i = 0; i < allSupportedLanguages.Length; i++)
            {
                if (allSupportedLanguages[i] == lang) return i;
            }
            return -1;
        }

        static public void nextLanguage()
        {
            IsoLanguages cur = getSavedIsoLanguage();

            int supportIndex = -1;
            for (int i = 0; i < allSupportedLanguages.Length; i++)
            {
                if (cur == allSupportedLanguages[i])
                {
                    supportIndex = i;
                }
            }

            Debug.Assert(supportIndex > -1, "unsupported language ?");

            supportIndex++;

            if (supportIndex >= allSupportedLanguages.Length)
            {
                supportIndex = 0;
            }

            cur = allSupportedLanguages[supportIndex];

            Debug.Log("next language is : " + cur + " / " + allSupportedLanguages.Length);

            setSavedLanguage(cur, true);
        }


        static IsoLanguages stringToIso(string lang)
        {
            string[] nms = Enum.GetNames(typeof(IsoLanguages));
            for (int i = 0; i < nms.Length; i++)
            {
                if (nms[i] == lang) return (IsoLanguages)i;
            }

            Debug.LogError("nope ; using fallback language");

            return languageFallback;
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.twoletterisolanguagename?view=net-5.0
        /// </summary>
        static IsoLanguages sysToIso(SystemLanguage sys)
        {
            switch (sys)
            {
                case SystemLanguage.English: return IsoLanguages.en;
                case SystemLanguage.French: return IsoLanguages.fr;
                case SystemLanguage.German: return IsoLanguages.de;
                case SystemLanguage.Italian: return IsoLanguages.it;
                case SystemLanguage.Chinese: return IsoLanguages.zh;
                case SystemLanguage.Portuguese: return IsoLanguages.po;
                case SystemLanguage.Spanish: return IsoLanguages.es;
                default:
                    Debug.LogWarning("language " + sys + " is not supported ; returning system");
                    break;
            }

            return languageFallback;
        }

        /// <summary>
        /// in : language
        /// out : label of language
        /// </summary>
        static public string isoToLabel(IsoLanguages lang)
        {
            return instance?.getContent("menu_" + lang.ToString());
        }

        static string sysToIsoString(SystemLanguage sys) => sysToIso(sys).ToString();

        static public bool isIsoLanguageSupported(IsoLanguages iso)
        {
            for (int i = 0; i < allSupportedLanguages.Length; i++)
            {
                if (allSupportedLanguages[i] == iso) return true;
            }
            return false;
        }

        static IsoLanguages getSystemLanguageToIso()
        {
            return sysToIso(Application.systemLanguage);
        }

        static public void setSavedLanguage(IsoLanguages iso, bool applySwap = false)
        {
            //how to save
            //...
            //LabySaveManager.getStream().setOption(LANG_PREFIX, (float)iso); // save

            if (applySwap) applyLanguage(iso); // apply
        }

        /// <summary>
        /// uses sys language as default
        /// </summary>
        static public IsoLanguages getSavedIsoLanguage()
        {
            SystemLanguage langDefault = Application.systemLanguage;

#if loca_en
    langDefault = SystemLanguage.English;
#elif loca_fr
    langDefault = SystemLanguage.French;
#elif local_zh
    langDefault = SystemLanguage.Chinese;
#endif

            //default value
            IsoLanguages lang = sysToIso(langDefault);

            //how to load
            //IsoLanguages lang = (IsoLanguages)LabySaveManager.getStream().getOption(LANG_PREFIX, (int)sysToIso(langDefault));

            if (!isIsoLanguageSupported(lang))
            {
                lang = getSystemLanguageToIso(); // sys OR fallback if sys is not supported
            }

            return lang;
        }

    }

}
