using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// some enum/const
/// </summary>
namespace fwp.localization
{
    public enum IsoLanguages
    {
        en, fr, de, es, it, po, ru, zh
    }

    /// <summary>
    /// 
    /// </summary>
    public interface iLanguageChangeReact
    {
        void onLanguageChange(string lang);
    }

    public static class ParserStatics
    {

        public const int HEADER_SKIP_LINE_COUNT = 3;
        public const char SPREAD_LINE_BREAK = '@';
        public const char CELL_LINE_BREAK = '|';
        public const char SPREAD_CELL_SEPARATOR = ',';
        public const char SPREAD_CELL_ESCAPE_VALUE = '"';

    }

    public static class LocalizationStatics
    {

        static public Rect GetTextMeshWidth(TextMesh mesh)
        {
            float charaBase = mesh.fontSize * mesh.characterSize;
            Rect _bounds = new Rect(0, 0, 0, 0);
            float width = 0;
            float height = 0;
            float minminY = 0;
            float maxmaxY = 0;

            char symbol;
            CharacterInfo info;

            List<Vector4> lines = new List<Vector4>();
            lines.Add(Vector4.zero);
            int lastLine = 0;

            for (int i = 0; i < mesh.text.Length; i++)
            {
                symbol = mesh.text[i];

                if (symbol == '\n')
                {
                    lines.Add(Vector3.zero);
                    lastLine++;
                    continue;
                }

                if (mesh.font.GetCharacterInfo(symbol, out info, mesh.fontSize, mesh.fontStyle))
                {
                    Vector4 currentLine = lines[lastLine];
                    currentLine.x += info.advance;
                    currentLine.y = Mathf.Min(currentLine.y, info.minY);
                    currentLine.z = Mathf.Max(currentLine.z, info.maxY);
                    lines[lastLine] = currentLine;
                }
            }
            //_bounds.position = mesh.transform.position;

            for (int i = 0; i < lines.Count; i++)
            {
                Vector4 currentLine = lines[i];
                width = Mathf.Max(width, lines[i].x);
                minminY = Mathf.Min(minminY, lines[i].y);
                maxmaxY = Mathf.Max(maxmaxY, lines[i].z);
                currentLine.w = Mathf.Abs(maxmaxY - minminY) * mesh.characterSize * 0.1f;
                lines[i] = currentLine;
            }

            float singleLineSpacing = mesh.lineSpacing * mesh.characterSize * mesh.fontSize * 0.11f; // I don't know why the 0.12, but it work ^^

            height = (lines.Count - 1) * singleLineSpacing + lines[0].w / 2 + lines[lines.Count - 1].w / 2;
            //height = (lines.Count - 1) * singleLineSpacing;
            maxmaxY = maxmaxY * mesh.characterSize * 0.1f;
            minminY = minminY * mesh.characterSize * 0.1f;
            _bounds.width = width * mesh.characterSize * 0.1f; // placeholder

            _bounds.y += charaBase + ((minminY + maxmaxY) / 2);



            //_bounds.height = Mathf.Abs(maxmaxY - minminY); ;
            _bounds.height = height;

            _bounds.size = new Vector2(_bounds.size.x * mesh.transform.lossyScale.x, _bounds.size.y * mesh.transform.lossyScale.y);

            return _bounds;
        }


#if UNITY_EDITOR

        static public ScriptableObject[] getScriptableObjectsInEditor(System.Type scriptableType)
        {
            string[] all = AssetDatabase.FindAssets("t:" + scriptableType.Name);

            List<ScriptableObject> output = new List<ScriptableObject>();
            for (int i = 0; i < all.Length; i++)
            {
                Object obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(all[i]), scriptableType);
                ScriptableObject so = obj as ScriptableObject;
                if (so == null) continue;
                output.Add(so);
            }

            //Debug.Log(scriptableType + " x"+output.Count+" / x" + all.Length);

            return output.ToArray();
        }


        static public ScriptableObject[] getScriptableObjectsInEditor<T>() where T : ScriptableObject
            => getScriptableObjectsInEditor(typeof(T));

        static public T getScriptableObjectInEditor<T>(string nameContains = "") where T : ScriptableObject
        {
            string[] all = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            for (int i = 0; i < all.Length; i++)
            {
                Object obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(all[i]), typeof(T));
                T data = obj as T;

                if (data == null) continue;
                if (nameContains.Length > 0)
                {
                    if (!data.name.Contains(nameContains)) continue;
                }

                return data;
            }
            Debug.LogWarning("can't locate scriptable of type " + typeof(T).Name + " (filter name ? " + nameContains + ")");
            return null;
        }
#endif


    }
}