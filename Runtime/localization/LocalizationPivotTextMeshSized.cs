using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace fwp.localization
{
    public class LocalizationPivotTextMeshSized : MonoBehaviour
    {
        [Serializable]
        public struct LocalizationPivotTextOffset
        {
            public TextMesh txt;
            public Vector2 localOffset;
        }

        [Serializable]
        public struct LocalizationLangTextSize
        {
            public string langName;
            public int size;
            public Vector2 globalOffset;
        }

        public List<LocalizationPivotTextOffset> textOffsets = new List<LocalizationPivotTextOffset>();
        public List<LocalizationLangTextSize> langSizes = new List<LocalizationLangTextSize>();

        [ContextMenu("fetch refs")]
        protected void fetch()
        {
            IsoLanguages[] langs = LocalizationManager.allSupportedLanguages;
            TextMesh[] tmeshs = transform.GetComponentsInChildren<TextMesh>();

            textOffsets.Clear();
            for (int i = 0; i < tmeshs.Length; i++)
            {
                LocalizationPivotTextOffset data = new LocalizationPivotTextOffset();
                data.txt = tmeshs[i];
                data.localOffset = Vector2.zero;
                textOffsets.Add(data);
            }

            langSizes.Clear();
            for (int j = 0; j < langs.Length; j++)
            {
                LocalizationLangTextSize ts = new LocalizationLangTextSize();
                ts.langName = langs[j].ToString();
                ts.size = textOffsets[0].txt.fontSize;
                langSizes.Add(ts);
            }

        }

        protected void applyData()
        {
            foreach (LocalizationPivotTextOffset textData in textOffsets)
            {
                LocalizationLangTextSize textSize = langSizes[LocalizationManager.getLanguageIndex()];

                Vector3 pos = textSize.globalOffset + textData.localOffset;
                pos.z = textData.txt.transform.localPosition.z;
                textData.txt.transform.localPosition = pos;

                textData.txt.fontSize = textSize.size;
            }
        }

        protected void onNewLangContent(string langContent)
        {
            foreach (LocalizationPivotTextOffset textData in textOffsets)
            {
                if (textData.txt != null) textData.txt.text = langContent;
            }

            applyData();
        }

        private void OnDrawGizmosSelected()
        {
            applyData();
        }

    }

}
