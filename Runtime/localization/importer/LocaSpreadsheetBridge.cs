using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Microsoft.VisualBasic;

/// <summary>
/// https://docs.google.com/spreadsheets/d/1UMlSlXTysA39zt0yRJljwTX2dPp5vQOo9plhI7YYCVU/edit#gid=1605195944
/// https://docs.google.com/spreadsheets/d/1UMlSlXTysA39zt0yRJljwTX2dPp5vQOo9plhI7YYCVU/export?format=tsv&gid=1605195944
/// </summary>

namespace fwp.localization
{

    public class LocaSpreadsheetBridge : MonoBehaviour
    {

        public const string sheetUrlPrefix = "https://docs.google.com/spreadsheets/d/";

        /// <summary>
        /// returns raw text from exported file
        /// </summary>
        static public string ssheet_import(string file_id, string sheet_id = "", bool logs = false)
        {
            //string[] split = null;

            //you can get this url within the publishing settings
            string ssheetUrl = sheetUrlPrefix + file_id + "/export?format=csv";

            if (sheet_id.Length > 0) ssheetUrl += "&gid=" + sheet_id;

            UnityWebRequest uwr = UnityWebRequest.Get(ssheetUrl);
            //uwr.url = ssheetUrl;

            Debug.Log("query : " + uwr.url);

            float time = Time.realtimeSinceStartup;

            UnityWebRequestAsyncOperation async = uwr.SendWebRequest();
            while (!async.isDone) ;
            if (uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(uwr.error);
                return string.Empty;
            }

            Debug.Log("query took : " + (Time.realtimeSinceStartup - time) + " second(s)");

            string txt = uwr.downloadHandler.text;

            if (txt == null)
            {
                Debug.LogError("null text");
                return string.Empty;
            }

            if (txt.Length <= 0)
            {
                Debug.LogError("empty text");
                return string.Empty;
            }

            Debug.Log("RAW file:" + file_id + " , sheet:" + sheet_id + " , content len:" + txt.Length);
            //Debug.Log(txt);

            //split = LocalizationFile.splitLineBreak(txt);
            //Debug.Log("lines x" + split.Length);
            //for (int i = 0; i < split.Length; i++) Debug.Log(split[i]);

            //https://stackoverflow.com/questions/22185009/split-text-with-r-n

            // en fonction de qui a fait les retour a la ligne dans le excel (mac vs windows) c'est pas consistent
            // parfois on a des \n propre et parfois c'est un mix de \n et \r\n

            //https://stackoverflow.com/questions/3147836/c-sharp-regex-split-commas-outside-quotes
            //Regex regx = new Regex(',' + "(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            //string[] line = regx.Split(txt);

            return txt;
        }

    }

}
