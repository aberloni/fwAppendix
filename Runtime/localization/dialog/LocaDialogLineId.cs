using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.localization
{
    [System.Serializable]
    public class LocaDialogLineId
    {
        //SPREADSHEET ID
        [SerializeField]
        string uid = ""; // l'uid raw

        [SerializeField]
        string fuid = ""; // readonly, l'uid solutionné après les patterns

        //FOR DEBUG ONLY
        [SerializeField]
        readonly string[] _previews = new string[LocalizationManager.allSupportedLanguages.Length];

        public LocaDialogLineId(string uid)
        {
            this.uid = uid;
            this.fuid = uid;
        }

        [ContextMenu("debug update preview")]
        public void debugUpdatePreview()
        {
            for (int i = 0; i < _previews.Length; i++)
            {
                _previews[i] = getSolvedLineByUID(false);
            }
        }

        public string getUID() => uid;

        //THIS IS WHAT SHOULD PROVIDE LOCA
        public string getSolvedLineByUID(bool useFallback = false)
        {
            return useFallback ? LocalizationManager.get().getContentSafe(uid) : LocalizationManager.get().getContent(uid);
        }

        public string getSolvedLineByFUID() => LocalizationManager.get().getContent(fuid);

        /// <summary>
        /// do not use
        /// not working
        /// </summary>
        public string getLineUsingPatterns(string[] patterns, string[] replaces)
        {
            Debug.Assert(patterns.Length == replaces.Length);

            string tmp = uid;
            for (int i = 0; i < patterns.Length; i++)
            {
                tmp = tmp.Replace(patterns[i], replaces[i]);
            }

            fuid = tmp;

            return getSolvedLineByFUID();
        }

        public bool hasUID()
        {
            //for loading issues .......
            if (uid == null) return false;

            return uid.Length > 0;
        }

        /// <summary>
        /// generic filtering of specific cases
        /// </summary>
        static public string getSolvedLocaLine(LocaDialogLineId field, bool fallbackIfMissing = false, bool emptyOnMissing = false)
        {
            return getFilteredLocaLine(field.getSolvedLineByUID(fallbackIfMissing), emptyOnMissing);
        }

        static public string getLocaByUID(string uid, bool fallbackIfMissing = false, bool emptyOnMissing = false)
        {
            string content = fallbackIfMissing ? LocalizationManager.get().getContentSafe(uid) : LocalizationManager.get().getContent(uid);
            return getFilteredLocaLine(content, emptyOnMissing);
        }

        static public string getFilteredLocaLine(string contentLocalized, bool emptyOnMissing = false)
        {
            // 2 parce qu'il y a deux charactères qui trainent dans l'export
            if (contentLocalized.Length <= 1)
            {
                contentLocalized = "[empty]";
                //Debug.LogWarning("no text given ?");
            }

            //missing ?
            if (contentLocalized.Length > 0)
            {
                if (contentLocalized[0] == '[')
                {
                    if (emptyOnMissing) contentLocalized = string.Empty;
                }
            }

            return contentLocalized;
        }

    }
}
