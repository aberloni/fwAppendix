using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace fwp.localization
{
    static public class LocalizationMenuItems
    {

        [MenuItem("Tools/Localization/ppref/de")] public static void pprefDE() => editor_switchLanguage(IsoLanguages.de, false);
        [MenuItem("Tools/Localization/ppref/en")] public static void pprefEN() => editor_switchLanguage(IsoLanguages.en, false);
        [MenuItem("Tools/Localization/ppref/es")] public static void pprefES() => editor_switchLanguage(IsoLanguages.es, false);
        [MenuItem("Tools/Localization/ppref/fr")] public static void pprefFR() => editor_switchLanguage(IsoLanguages.fr, false);
        [MenuItem("Tools/Localization/ppref/it")] public static void pprefIT() => editor_switchLanguage(IsoLanguages.it, false);
        [MenuItem("Tools/Localization/ppref/po")] public static void pprefPO() => editor_switchLanguage(IsoLanguages.po, false);
        [MenuItem("Tools/Localization/ppref/ru")] public static void pprefRU() => editor_switchLanguage(IsoLanguages.ru, false);
        [MenuItem("Tools/Localization/ppref/cn")] public static void pprefZH() => editor_switchLanguage(IsoLanguages.zh, false);

        [MenuItem("Tools/Localization/swap/deu")] public static void swapDE() => editor_switchLanguage(IsoLanguages.de);
        [MenuItem("Tools/Localization/swap/eng")] public static void swapEN() => editor_switchLanguage(IsoLanguages.en);
        [MenuItem("Tools/Localization/swap/esp")] public static void swapES() => editor_switchLanguage(IsoLanguages.es);
        [MenuItem("Tools/Localization/swap/fre")] public static void swapFR() => editor_switchLanguage(IsoLanguages.fr);
        [MenuItem("Tools/Localization/swap/ita")] public static void swapIT() => editor_switchLanguage(IsoLanguages.it);
        [MenuItem("Tools/Localization/swap/por")] public static void swapPO() => editor_switchLanguage(IsoLanguages.po);
        [MenuItem("Tools/Localization/swap/rus")] public static void swapRU() => editor_switchLanguage(IsoLanguages.ru);
        [MenuItem("Tools/Localization/swap/chi")] public static void swapZH() => editor_switchLanguage(IsoLanguages.zh);

        public static void editor_switchLanguage(IsoLanguages newLang, bool swap = true)
        {
            LocalizationManager.setSavedLanguage(newLang, swap);
        }
    }

}
