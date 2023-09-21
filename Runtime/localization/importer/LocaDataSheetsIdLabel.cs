using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

namespace fwp.localization
{
    /// <summary>
    /// base structure to get a scriptble with uid/tabid couples
    /// </summary>
    abstract public class LocaDataSheetsIdLabel : ScriptableObject
    {
        public string sheetUrl;
        public DataSheetLabel[] sheetTabsIds;

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if(sheetUrl.StartsWith(LocaSpreadsheetBridge.sheetUrlPrefix))
            {
                sheetUrl = sheetUrl.Replace(LocaSpreadsheetBridge.sheetUrlPrefix, string.Empty);
            }
        }

        public string getMatchingLabel(string tabId) => getTabIdOfField(tabId).ToString().ToLower();

        public string getTabIdOfField(string field)
        {
            for (int i = 0; i < sheetTabsIds.Length; i++)
            {
                if (sheetTabsIds[i].fieldId == field) return sheetTabsIds[i].tabId;
            }
            return string.Empty;
        }

        virtual protected DataSheetLabel[] getAdditionnalLabels() => null;

        public DataSheetLabel[] getAllTabs()
        {
            List<DataSheetLabel> output = new List<DataSheetLabel>();

            output.AddRange(sheetTabsIds);

            var addi = getAdditionnalLabels();
            if(addi != null)
            {
                output.AddRange(addi);
            }

            return output.ToArray();
        }
    }

    [Serializable]
    public struct DataSheetLabel
    {
        public string fieldId; // tab name in sheet
        public string tabId; // ssheet url uid
    }

}
