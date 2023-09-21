using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;


namespace fwp.localization
{

    /// <summary>
    ///  Text Assets are a format for imported text files. When you drop a text file into your Project Folder, it will be converted to a Text Asset. The supported text formats are:
    ///     .txt
    ///     .html
    ///     .htm
    ///     .xml
    ///     .bytes
    ///     .json
    ///     .csv
    ///     .yaml
    ///     .fnt
    ///     
    /// structure
    /// 
    /// 1
    /// 00:01:26,880 --> 00:01:32,160
    /// J'ai commencé la photo en 2011-2012 environ.
    /// 
    /// 2
    /// 00:01:33,050 --> 00:01:38,510
    /// J'ai débuté avec un appareil classique. Je faisais beaucoup de photos de rue.
    /// 
    /// </summary>

    public class LocalizationSubtitlesFile
    {

        TextAsset ta;

        List<LocalizationSubtitleLine> lines = new List<LocalizationSubtitleLine>();

        TMPro.TextMeshProUGUI txt;

        const string locaSubPath = "localization/subtitles/";

        public LocalizationSubtitlesFile(string videoFileName)
        {
            setupForVideo(videoFileName);
        }

        public void setupForVideo(string videoFileName)
        {
            string path = Path.Combine(locaSubPath, videoFileName);

            ta = Resources.Load(path) as TextAsset;

            //https://docs.unity3d.com/Manual/class-TextAsset.html
            if (ta == null)
            {
                Debug.LogError("no sub at : " + path);
                return;
            }

            //DO NOT REMOVE EMPTY LINES (separator)
            //MACOS : NewLine va bien séparer les lignes mais il va rester le '\r' linux qui compte comme un char
            //string[] rawLines = ta.text.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.None);

            //https://stackoverflow.com/questions/1508203/best-way-to-split-string-into-lines
            string[] rawLines = Regex.Split(ta.text, "\r\n|\r|\n");

            if (rawLines.Length <= 0) Debug.LogWarning("no lines from raw text");
            else
            {
                Debug.Log("solving sub(s) out of x" + rawLines.Length + " lines");

                lines = new List<LocalizationSubtitleLine>();

                List<string> tmp = new List<string>();

                //tmp will gather data until an empty line is reached
                for (int i = 0; i < rawLines.Length; i++)
                {
                    string line = rawLines[i];

                    //Debug.Log("  --> (" + line.Length + ") " + line);

                    if (line.Length <= 0) // search for empty line (might have line endings !) ...
                    {
                        //has gathered content ? creating sub with it
                        if (tmp.Count > 2)
                        {
                            lines.Add(new LocalizationSubtitleLine(tmp));
                            //Debug.Log("added new sub ! new total : x" + lines.Count);
                        }
                        tmp.Clear();
                    }
                    else tmp.Add(line);
                }

            }

#if UNITY_EDITOR
            Debug.Log("<b>loaded subtitles</b> for video : " + videoFileName + " | at path : " + path + " | lines count : " + lines.Count);
            if (lines.Count > 0) Debug.Log("<b>loaded subtitles</b>  L first line : " + lines[0].buffLine);
            else Debug.LogWarning("no subtitles will be displayed ...");
#endif

            //for (int i = 0; i < lines.Count; i++) Debug.Log("  " + lines[i].line);
        }

        public void assignTextfield(TMPro.TextMeshProUGUI txt)
        {
            this.txt = txt;

            txt.text = string.Empty;
            txt.enabled = true;

            //Debug.Log("setupAndStart");
        }

        public void update(double timecode)
        {
            if (lines.Count <= 0)
            {
                Debug.LogWarning("sub file has x0 lines to match timecode " + timecode);
                return;
            }

            txt.text = string.Empty;

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].isWithingLineTimecodeRange(timecode))
                {
                    txt.text = lines[i].buffLine;

                    //Debug.Log("sub | time?"+timecode + " , text:" + txt.text);
                }
            }

        }

        public bool isValid()
        {
            return true;
        }

    }

    /// <summary>
    /// 
    /// 1
    /// 00:01:26,880 --> 00:01:32,160
    /// J'ai commencé la photo en 2011-2012 environ.
    /// 
    /// </summary>
    public class LocalizationSubtitleLine
    {
        //int id = -1;
        float timecode_start = 0f;
        float timecode_end = 0f;

        public string rawLine = string.Empty; // actual value stored in file
        public string buffLine = string.Empty; // translated content from rawLine id

        public LocalizationSubtitleLine(List<string> rawLines)
        {
            //id = int.Parse(rawLines[0]);

            //Debug.Log("  generated subtitle line #" + id);

            string timecode = rawLines[1];
            string[] tmp;
            string[] fields;

            //for (int i = 0; i < rawLines.Count; i++) Debug.Log(rawLines[i]);

            //start
            tmp = timecode.Split(' '); // HH:MM:SS,MMM
            fields = tmp[0].Split(':');
            timecode_start = int.Parse(fields[0]) * 60f * 60f; // HH
            timecode_start += int.Parse(fields[1]) * 60f; // MM
            fields = fields[2].Split(',');
            timecode_start += int.Parse(fields[0]); // SS
            timecode_start += int.Parse(fields[1]) / 1000f; // MMM -> SS

            //Debug.Log(timecode + " --> " + timecode_start);

            //end
            fields = tmp[tmp.Length - 1].Split(':');
            timecode_end = int.Parse(fields[0]) * 60f * 60f; // HH
            timecode_end += int.Parse(fields[1]) * 60f; // MM
            fields = fields[2].Split(',');
            timecode_end += int.Parse(fields[0]); // SS
            timecode_end += int.Parse(fields[1]) / 1000f; // MMM -> SS

            //Debug.Log(timecode + " --> " + timecode_end);

            //content
            rawLine = rawLines[2];
            buffLine = LocalizationManager.get().getContent(rawLine, true);

            Debug.Log("  generated subtitle line #" + rawLine + " [" + timecode_start + " , " + timecode_end + "]   : " + buffLine);
        }

        public bool isWithingLineTimecodeRange(double timecode)
        {
            //Debug.Log(timecode + " ? " + timecode_start + " , " + timecode_end);

            return (timecode_start < timecode && timecode_end > timecode);
        }
    }
}