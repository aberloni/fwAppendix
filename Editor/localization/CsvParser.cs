using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

/// <summary>
/// MAISONNNN
/// </summary>

namespace fwp.localization.editor
{
    public class CsvParser
    {
        string originalRaw;

        public struct CsvLine
        {
            public string raw;
            public List<string> cell;

            public string logify()
            {
                string output = string.Empty;
                for (int i = 0; i < cell.Count; i++)
                {
                    if (i > 0) output += ParserStatics.SPREAD_CELL_SEPARATOR;
                    output += cell[i];
                }
                return output;
            }
        }

        /// <summary>
        /// each line is a variant of localization/language
        /// </summary>
        public List<CsvLine> lines = new List<CsvLine>();

        public CsvParser(string raw)
        {
            originalRaw = raw;
            //Debug.Log(raw);

            string[] rawLines = raw.Split(new char[] { ParserStatics.SPREAD_LINE_BREAK }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = ParserStatics.HEADER_SKIP_LINE_COUNT; i < rawLines.Length; i++)
            {
                CsvLine line = new CsvLine();
                line.cell = new List<string>();
                line.raw = rawLines[i];

                string[] split = cellsSeparator(rawLines[i]);

                int cntCellWithContent = 0;
                for (int j = 0; j < split.Length; j++)
                {
                    if (split[j].Length > 0)
                    {
                        //Debug.Log(split[j] + " (" + split[j].Length + ")");
                        cntCellWithContent++;
                    }
                    line.cell.Add(split[j]);
                }

                //skip line of only empty cells
                if (cntCellWithContent > 1) lines.Add(line);
            }

            if(ExportLocalisationToGoogleForm.verbose)
                Debug.Log("csv solved x" + lines.Count);
        }

        string[] cellsSeparator(string lineRaw)
        {
            bool inValue = false;

            List<string> cells = new List<string>();

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < lineRaw.Length; i++)
            {
                char cur = lineRaw[i];

                if (cur == ParserStatics.SPREAD_CELL_ESCAPE_VALUE) // "
                {
                    inValue = !inValue;
                }

                if (cur == ParserStatics.SPREAD_CELL_SEPARATOR && !inValue)
                {
                    cells.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    //don't add separator "," symbol
                    sb.Append(cur);
                }



            }

            return cells.ToArray();
        }

        public string getCleanedCsvText()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < lines.Count; i++)
            {
                sb.AppendLine(lines[i].logify());
            }

            return sb.ToString();
        }

        static public CsvParser parse(string raw)
        {

            //search & replace all LINE breaks
            // a line break char OUTSIDE of ""

            StringBuilder sb = new StringBuilder(raw);

            bool inCellValue = false;
            for (int i = 0; i < sb.Length; i++)
            {
                char cur = sb[i];

                if (cur == '"') inCellValue = !inCellValue;

                if (isCharLineBreak(cur.ToString()))
                {
                    cur = inCellValue ? ParserStatics.CELL_LINE_BREAK : ParserStatics.SPREAD_LINE_BREAK;
                }

                sb[i] = cur;
            }

            //quand on tombe sur \r puis \n ça fait @@
            sb.Replace("@@", "@");
            sb.Replace("||", "|");

            raw = sb.ToString();

            return new CsvParser(raw);
        }

        static bool isCharLineBreak(string cur)
        {
            if (cur == System.Environment.NewLine) return true;
            if (cur == "\r\n") return true;
            if (cur == "\r") return true;
            if (cur == "\n") return true;
            return false;
        }

    }

}
