using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace fwp.localization.editor
{
    public class ExportLocalisationToGoogleForm
    {
        static public bool verbose = false;

        const int KEY_COLUMN = 2;

        static public void ssheet_import(LocaDataSheetsIdLabel sheet)
        {
            if (sheet == null)
            {
                Debug.LogError("no scriptable with tabs ids ?");
                return;
            }

            DataSheetLabel[] tabs = sheet.getAllTabs();

            EditorUtility.DisplayProgressBar("importing loca", "fetching...", 0f);

            for (int i = 0; i < tabs.Length; i++)
            {
                EditorUtility.DisplayProgressBar("importing loca", tabs[i].fieldId + "&" + tabs[i].tabId, (1f * i) / (1f * tabs.Length));
                importAndSaveSheetTab(sheet.sheetUrl, tabs[i]);
            }

            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh();
        }

        static public void trad_files_generation()
        {
            Debug.Log("generating for x" + LocalizationManager.allSupportedLanguages.Length + " languages");

            EditorUtility.DisplayProgressBar("converting loca", "loading...", 0f);

            for (int i = 0; i < LocalizationManager.allSupportedLanguages.Length; i++)
            {
                IsoLanguages lang = LocalizationManager.allSupportedLanguages[i];

                EditorUtility.DisplayProgressBar("converting loca", "langage : " + lang.ToString(), (1f * i) / (1f * LocalizationManager.allSupportedLanguages.Length));

                trad_file_generate(lang);
            }

            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh();
        }

        [MenuItem("Localization/import/solve all dialog lines")]
        static protected void solveLines()
        {
            LocaDialogData[] all = (LocaDialogData[])LocalizationStatics.getScriptableObjectsInEditor<LocaDialogData>();

            bool hasChanged = false;

            float progress = 0f;
            for (int i = 0; i < all.Length; i++)
            {
                progress = (float)(i + 1f) / Mathf.Max(1f, (float)all.Length);
                if (EditorUtility.DisplayCancelableProgressBar("Solving all dialog lines", "Solving " + all[i].name + " (" + (i + 1) + "/" + all.Length + ")", progress))
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }

                all[i].cmSolveLines(out hasChanged);

                if (hasChanged)
                    EditorUtility.SetDirty(all[i]);
            }
            AssetDatabase.SaveAssets();

            EditorUtility.ClearProgressBar();
        }

        [MenuItem("Localization/import/solve all dialog lines NO DIFF")]
        static protected void solveLinesNoDiff()
        {
            LocaDialogData[] all = (LocaDialogData[])LocalizationStatics.getScriptableObjectsInEditor<LocaDialogData>();

            bool hasChanged = false;

            float progress = 0f;
            for (int i = 0; i < all.Length; i++)
            {
                progress = (float)(i + 1f) / Mathf.Max(1f, (float)all.Length);
                if (EditorUtility.DisplayCancelableProgressBar("Solving all dialog lines", "Solving " + all[i].name + " (" + (i + 1) + "/" + all.Length + ")", progress))
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }

                all[i].cmSolveLines(out hasChanged);

                EditorUtility.SetDirty(all[i]);
            }
            AssetDatabase.SaveAssets();

            EditorUtility.ClearProgressBar();
        }

        [MenuItem("Localization/import/generate only EN")]
        static void gen_only_en()
        {
            trad_file_generate(IsoLanguages.en);
        }

        [MenuItem("Localization/import/download AND generate")]
        static void ssheet_down_n_generate()
        {
            ssheet_import(LocalizationManager.get().getSheetLabels());
            trad_files_generation();
        }

        /// <summary>
        /// merge all tabs into a single file for given language
        /// </summary>
        /// <param name="lang"></param>
        static protected void trad_file_generate(IsoLanguages lang)
        {
            string importPath = Path.Combine(Application.dataPath, 
                LocalizationManager.path_resource_localization, "import");

            string[] tabsFiles = Directory.GetFiles(importPath, "*.txt");

            Debug.Log(" generating trad file for : <b>" + lang.ToString().ToUpper() + "</b>");

            StringBuilder output = new StringBuilder();

            for (int i = 0; i < tabsFiles.Length; i++)
            {
                string tabOutput = solveTab(tabsFiles[i], lang);
                output.AppendLine(tabOutput);
            }

            //save

            string outputPath = Path.Combine(Application.dataPath, 
                LocalizationManager.path_resource_localization, "lang_" + lang + ".txt");

            if(verbose)
                Debug.Log("saving : " + outputPath + " (" + output.Length + " char)");

            File.WriteAllText(outputPath, output.ToString());
        }

        /// <summary>
        /// solving whatever was saved in raw files
        /// </summary>
        static string solveTab(string filePath, IsoLanguages lang)
        {
            string tabContent = File.ReadAllText(filePath);

            if(verbose)
                Debug.Log("parsing csv file : " + filePath);

            CsvParser csv = CsvParser.parse(tabContent);

            if (verbose)
                Debug.Log("  solved x" + csv.lines.Count + " lines after CSV parser");

            //for (int j = 0; j < lines.Length; j++) { Debug.Log("#" + j); Debug.Log(lines[j]); }

            //  search for mathing language column

            //after CSV treatment on import, header is removed, language are on first line
            int LANGUAGE_LINE_INDEX = 0; // languages are stored at line 5
            string[] langs = csv.lines[LANGUAGE_LINE_INDEX].cell.ToArray(); // each lang of cur line

            int langColumnIndex = -1;
            for (int j = 0; j < langs.Length; j++)
            {
                // is the right language ?
                if (langs[j].Trim().ToLower() == lang.ToString().ToLower())
                {
                    langColumnIndex = j;
                    //Debug.Log("found lang "+lang+" at column #"+ langColumnIndex);
                }
            }

            if (langColumnIndex < 0)
            {
                Debug.LogWarning("sheet import : <b>no column</b> for lang : <b>" + lang.ToString().ToUpper() + "</b> | out of x" + langs.Length);

                if (verbose)
                {
                    Debug.LogWarning(csv.lines[LANGUAGE_LINE_INDEX].raw);
                    for (int i = 0; i < langs.Length; i++)
                    {
                        Debug.LogWarning(langs[i]);
                    }
                }

                return string.Empty;
            }

            int cntNotTranslation = 0;

            StringBuilder output = new StringBuilder();
            string langValue;

            //first line is languages
            for (int j = 1; j < csv.lines.Count; j++)
            {
                string[] datas = csv.lines[j].cell.ToArray();

                //Debug.Log(j + " => x" + datas.Length);
                //Debug.Log(line);

                //here filter everything NOT grab from excel file
                if (datas.Length < 2) continue; // empty line
                if (datas[KEY_COLUMN].Length < 1) continue; // key empty

                //ligne vide, pas assez de colonnes dedans ?
                if (langColumnIndex >= datas.Length)
                {
                    Debug.LogError("line #" + j + " => line has not enought cells for a lang in column #" + langColumnIndex + " / out of x" + datas.Length + " columns");
                    Debug.Log(csv.lines[j].raw);
                    continue;
                }

                string val = datas[KEY_COLUMN];
                if (val.Contains(" ")) // pas d'espace dans les ids
                {
                    Debug.LogWarning("skipping value (with spaces) : " + val);
                    continue; // skip line with spaces
                }

                //https://www.c-sharpcorner.com/uploadfile/mahesh/trim-string-in-C-Sharp/
                // remove white spaces on sides
                string key = val.Trim();

                //langValue = langValue.Replace(CsvParser.CELL_LINE_BREAK, System.Environment.NewLine);
                langValue = datas[langColumnIndex];

                //remove "" around escaped values
                langValue = langValue.Replace(ParserStatics.SPREAD_CELL_ESCAPE_VALUE.ToString(), string.Empty);

                // note : line return IN cells should be replaced by | here
                output.AppendLine(key + "=" + langValue);
            }

            if (cntNotTranslation > 0)
            {
                Debug.LogWarning("x" + cntNotTranslation + " lines have NO translation in " + lang);
            }

            return output.ToString();
        }

        /// <summary>
        /// where the download and treatment of original CSV is done
        /// </summary>
        static protected void importAndSaveSheetTab(string sheetUrl, DataSheetLabel dt)
        {
            //fileContent is raw downloadHandler text
            string fileContent = LocaSpreadsheetBridge.ssheet_import(sheetUrl, dt.tabId);

            string outputFolder = Path.Combine(Application.dataPath, LocalizationManager.path_resource_localization, "import");

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            //string fileName = getTabIdFileName(tabId);
            string fileName = dt.fieldId + "_" + dt.tabId;

            string filePath = Path.Combine(outputFolder, fileName + ".txt");
            File.WriteAllText(filePath, fileContent);

            //FileStream stream = File.OpenRead(filePath);
            //stream.Close();

            Debug.Log("  saved : <b>" + fileName + "</b> ; chars saved in file : " + fileContent.Length);
        }

        static protected string getTabIdFileName(string tabId)
        {
            LocaDataSheetsIdLabel data = LocalizationStatics.getScriptableObjectInEditor<LocaDataSheetsIdLabel>();

            if (data == null) return string.Empty;

            return data.getMatchingLabel(tabId);
        }

    }

}
