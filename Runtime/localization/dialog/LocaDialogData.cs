using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.localization
{
    /// <summary>
    /// dialog UID is name of scriptable
    /// </summary>
    [CreateAssetMenu(menuName = "dialogs/create LocaDialogData", fileName = "DialogData_", order = 100)]
    public class LocaDialogData : ScriptableObject
    {
        public string locaId;

        [SerializeField]
        public LocaDialogLineData[] lines;

        public LocaDialogLineData getNextLine(LocaDialogLineData line)
        {
            Debug.Assert(line != null);

            Debug.Log(GetType() + " :: " + name + " :: getNextLine() :: searching next line, in x" + lines.Length + " possible lines");
            Debug.Log("  L from line " + line.lineId.getUID() + " (" + line.lineId.getSolvedLineByUID(false) + ")");

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == line)
                {
                    Debug.Log("  L found line at index " + i + ", returning next LineData");

                    // last, no next
                    if (i >= lines.Length - 1) return null;

                    return lines[i + 1];
                }
            }

            Debug.LogWarning("  L /! line wasn't in dialog ?");
            return null;
        }

#if UNITY_EDITOR
        [ContextMenu("solve lines")]
        public void cmSolveLines()
        {
            bool osef = false;
            cmSolveLines(out osef);
        }

        public void cmSolveLines(out bool hasChanged)
        {
            hasChanged = false;

            if (locaId == null)
            {
                Debug.LogWarning(name + " has no loca id", this);
                return;
            }

            if (locaId.Length <= 0)
            {
                Debug.LogWarning(name + " loca id is empty", this);
                return;
            }

            List<LocaDialogLineData> tmp = new List<LocaDialogLineData>();

            int safe = 50;
            int index = 1;
            string ct;

            do
            {
                string fullId = locaId + "_" + ((index < 10) ? "0" + index : index.ToString());
                ct = LocalizationManager.get().getContent(fullId);

                //Debug.Log("(solving lines) fid ? " + fullId + " => " + ct);

                if (ct.IndexOf("['") > -1) ct = string.Empty;

                if (ct.Length > 0)
                {
                    LocaDialogLineData line = new LocaDialogLineData(fullId);
                    tmp.Add(line);
                }

                index++;
                safe--;

            } while (safe > 0 && ct.Length > 0);


            string mergeLog = string.Empty;

            if (lines == null || lines.Length <= 0)
            {
                if (tmp.Count > 0)
                {
                    lines = tmp.ToArray();
                    hasChanged = true;
                }
            }
            else
            {
                List<LocaDialogLineData> merged = new List<LocaDialogLineData>();

                for (int i = 0; i < tmp.Count; i++)
                {
                    if (i < lines.Length)
                    {
                        if (lines[i].lineId.getSolvedLineByFUID() == tmp[i].lineId.getSolvedLineByFUID())
                            merged.Add(lines[i]);
                        else
                        {
                            merged.Add(tmp[i]);
                            hasChanged = true;
                        }
                    }
                    else
                    {
                        merged.Add(tmp[i]);
                        hasChanged = true;
                    }
                }

                lines = merged.ToArray();

                mergeLog = "[Merged]";
            }

            cmUpdateCached();

            if (hasChanged)
                Debug.Log("(solving lines) solved x" + tmp.Count + " lines for " + locaId + " " + mergeLog + " - changed =" + hasChanged);
        }

        [ContextMenu("update cached")]
        protected void cmUpdateCached()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i].debugUpdateCached();
            }
        }

        [ContextMenu("use name as loca id")]
        protected void cmUseNameAsId()
        {
            locaId = (name.Replace("LabyDialogData_", "")).Replace("-", "_env-");

            cmSolveLines();

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

    }

}
