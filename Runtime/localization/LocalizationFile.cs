using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace fwp.localization
{
    /// <summary>
    /// La couche qui permet de manipuler un fichier de loca
    /// 
    /// quand on download en CSV les \r\n sont les retours a la ligne des lignes du excel
    /// les \n seul sont les retour a la ligne dans la valeur d'une cellule
    /// 
    /// </summary>
    public class LocalizationFile
    {
        public const string SPREADSHEET_LINE_BREAK = "\r\n"; // ce qui sépare deux lignes du excel
        public const char SPREADSHEET_CELL_BREAK = ','; // ce qui sépare les cellules d'une ligne du excel
        public const char SPREADSHEET_CELL_LINE_BREAK = '\n'; // le char de la valeur d'une cellule pour line break

        public const char LOCALIZ_CHAR_COMMENT = '#'; //id=content
        public const string LOCALIZ_CHAR_COMMENT2 = "\\\\"; //id=content
        public const char LOCALIZ_CHAR_SPLIT = '='; //id=content
        public const string LOCALIZ_MULTIPLE_BEGIN = "<multiple>"; //id=content
        public const string LOCALIZ_MULTIPLE_END = "</multiple>"; //id=content
        public const string FILESEPARATOR = "@";

        public string lang_name = ""; // fr, en, ...

        public TextAsset textAsset;

        string[] lines;

        public LocalizationFile(IsoLanguages lang)
        {
            Init(lang.ToString());
        }

        public string[] getLines() => lines;

        private void Init(string lang)
        {
            lang_name = lang;

            string langFilePath = getLangFilePath(lang, false); // path to lang file, no ext
            textAsset = Resources.Load(langFilePath) as TextAsset;

            //Debug.Log(ta.name);

            if (textAsset == null)
            {
                Debug.LogError("no file : " + lang + " , at path ? " + langFilePath);
                return;
            }

            lines = splitLineBreak(textAsset.text);

            List<string> tmp = new List<string>();
            bool multipleLine = false;
            bool firstMultipleLine = false;
            foreach (string line in lines)
            {
                if (line.StartsWith("" + LocalizationFile.LOCALIZ_CHAR_COMMENT)) continue;
                if (line.Length <= 1)
                {
                    if (multipleLine) tmp[tmp.Count - 1] += "\n";
                    else continue;
                }

                if (line.StartsWith(LOCALIZ_MULTIPLE_BEGIN)) { multipleLine = true; firstMultipleLine = true; continue; }
                if (line.StartsWith(LOCALIZ_MULTIPLE_END)) { multipleLine = false; continue; }

                if (multipleLine)
                {
                    if (firstMultipleLine)
                        tmp.Add(line);
                    else
                        tmp[tmp.Count - 1] += "\n" + line;
                    firstMultipleLine = false;
                }
                else
                    tmp.Add(line);
            }
            lines = tmp.ToArray();

            //Debug.Log("  " + path + " | " + lines.Length + " lines");
        }

        public void debugRefresh()
        {
            Init(lang_name);
        }


        /// <summary>
        /// Permet de comparer deux fichiers de langue pour indiquer si ils sont compatibles
        /// </summary>
        public bool compare(LocalizationFile other)
        {
            List<string> ids = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                string id = getIdAtLine(i);
                if (id.Length > 0) ids.Add(id);
            }

            bool output = false;

            bool found = false;
            for (int i = 0; i < ids.Count; i++)
            {
                found = false;
                for (int j = 0; j < other.lines.Length; j++)
                {
                    string id = other.getIdAtLine(j);
                    if (id == ids[i]) found = true;
                }
                if (!found)
                {
                    Debug.LogError("missing id " + ids[i] + " from file " + lang_name + " in file " + other.lang_name);
                    output = true; // error
                }
            }

            return output;
        }

        public string getContentById(string id, bool warning = false)
        {
            if (id == null) return "[no id]";

            if (id.Length <= 0)
            {
                Debug.LogWarning("no id given to gather content loca ?");
                return "[no id given / empty]";
            }

            //Debug.Log("searching for " + id);
            for (int i = 0; i < lines.Length; i++)
            {
                string key = lines[i].Split('=')[0];

                key = key.Trim();
                id = id.Trim();

                //srt_outro_museum_05 == srt_outro_museum_05
                //Debug.Log(key + " ("+key.Length+") == " + id+" ("+id.Length+")");

                if (key == id) return getContentAtLine(i);
            }

            if (warning) Debug.LogWarning("  ~loca manager~ getContentById() couldn't find trad for id  : <b>" + id + "</b>");

            return "['" + id + "' missing in " + lang_name + "]";
        }

        public string getContentAtLine(int idx)
        {
            if (!lines[idx].Contains("" + LocalizationFile.LOCALIZ_CHAR_SPLIT)) return "";
            string[] split = lines[idx].Split(LocalizationFile.LOCALIZ_CHAR_SPLIT);
            string output = "";
            for (int i = 1; i < split.Length; i++)
            {
                if (i != 1) output += "=";
                output += split[i];
            }

            //dans la trad on a des | pour faire des \n
            output = output.Replace(ParserStatics.CELL_LINE_BREAK.ToString(), System.Environment.NewLine);

            return output;
        }

        public string getIdAtLine(int idx)
        {
            if (!lines[idx].Contains("" + LocalizationFile.LOCALIZ_CHAR_SPLIT)) return "";
            string[] split = lines[idx].Split(LocalizationFile.LOCALIZ_CHAR_SPLIT);
            return split[0];
        }

        public int getLineCount()
        {
            return lines.Length;
        }

        public bool isLoaded()
        {
            return textAsset != null;
        }

        public bool overrideKey(string key, string newText)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].Contains(LOCALIZ_CHAR_SPLIT.ToString())) continue;
                if (!lines[i].Contains(key)) continue;

                string[] splited = lines[i].Split(LOCALIZ_CHAR_SPLIT);
                if (splited[0] != key) continue;
                if (splited[1] == newText) continue;
                splited[1] = newText;
                lines[i] = splited[0] + LOCALIZ_CHAR_SPLIT + splited[1];
                return true;
            }
            return false;
        }

        public void rewriteAsset()
        {
            StreamWriter file = new StreamWriter("Assets/Resources/" + getLangFilePath(lang_name));

            string[] rawLines = splitLineBreak(textAsset.text);
            int numberOfRewrite = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].Contains(LOCALIZ_CHAR_SPLIT.ToString())) continue;
                string[] splited = lines[i].Split(LOCALIZ_CHAR_SPLIT);

                for (int j = 0; j < rawLines.Length; j++)
                {
                    if (!rawLines[j].Contains(LOCALIZ_CHAR_SPLIT.ToString())) continue;
                    string[] rawSplited = rawLines[j].Split(LOCALIZ_CHAR_SPLIT);
                    if (rawSplited[0] != splited[0]) continue;
                    rawLines[j] = lines[i];
                    numberOfRewrite++;
                }
            }

            Debug.Log("rewrited " + numberOfRewrite + " lines");

            string allNew = "";
            for (int i = 0; i < rawLines.Length; i++)
            {
                allNew += rawLines[i] + Environment.NewLine;
            }

            file.Write(allNew);
            file.Close();
        }

        public string getLangFilePath(string lang, bool ext = true)
        {
            return LocalizationManager.folder_localization + "lang_" + lang + (ext ? ".txt" : string.Empty);
        }

        static public string[] splitLineBreak(string fileContent)
        {
            //return fileContent.Split(new char[] { '\r', '\n' }, StringSplitOptions.None);
            return fileContent.Split(new string[] { LocalizationFile.SPREADSHEET_LINE_BREAK }, StringSplitOptions.None);
        }

    }
}