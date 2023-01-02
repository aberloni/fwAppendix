using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace fwp.localization
{
    public class LocalizationPlugSized : MonoBehaviour
    {

        const float offset = -0.032355f;
        //const float offset = -0.033333333f;

        [Header("size params")]
        public bool alignBottom = false;
        public Rect sizeModif = new Rect(0, 0, 0, 0);
        public SpriteRenderer spriteBounds;

        public TextMesh txt_mesh;

        protected void onNewLangContent(string langContent)
        {
            resise();
        }

        public void resise()
        {

            Rect size = GetWidth(txt_mesh);

            Vector3 spriteSize = Vector3.zero;

            if (spriteBounds != null)
            {
                spriteSize = spriteBounds.bounds.size;
            }

            while (size.width < spriteSize.x + sizeModif.width && size.height < spriteSize.y + sizeModif.height)
            {
                txt_mesh.fontSize++;
                size = GetWidth(txt_mesh);
                //break;
            }

            while (size.width > spriteSize.x + sizeModif.width || size.height > spriteSize.y + sizeModif.height)
            {
                txt_mesh.fontSize--;
                size = GetWidth(txt_mesh);
                //break;
            }

            //Vector3 newPosInverse = (Vector3)size.position - transform.position;

            if (spriteBounds != null)
            {
                //txt_mesh.transform.position = spriteBounds.transform.position + (Vector3)sizeModif.position - newPosInverse;
                txt_mesh.transform.position = spriteBounds.transform.position + (Vector3)sizeModif.position - (Vector3)size.position;
            }
            else
            {
                txt_mesh.transform.position = (Vector3)sizeModif.position - (Vector3)size.position;
            }
            //txt_mesh.transform.Translate(transform.position - newPosInverse);
        }

        private void OnDrawGizmosSelected()
        {

            if (txt_mesh != null)
            {

                Rect val = GetWidth(txt_mesh);

                Gizmos.color = new Color(0.5f, 0, 0, 0.5f);

                Gizmos.DrawCube(txt_mesh.transform.position + (Vector3)val.position, val.size);
            }


        }


        public Rect GetWidth(TextMesh mesh)
        {
            float charaBase = offset * mesh.fontSize * mesh.characterSize;
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



        [ContextMenu("copy size of first")]
        protected void copyFirstSize()
        {
            LocalizationPlugSized ps = transform.parent.GetComponentInChildren<LocalizationPlugSized>();
            if (ps == null)
            {
                Debug.Log("no local plug sized ?");
                return;
            }

            //Debug.Log("first is " + ps.name);

            LocalizationPlugSized[] pss = transform.parent.GetComponentsInChildren<LocalizationPlugSized>();
            for (int i = 0; i < pss.Length; i++)
            {
                if (pss[i] != ps)
                {
                    pss[i].sizeModif = ps.sizeModif;

                }

                pss[i].resise();
            }

        }


        private void OnDrawGizmos()
        {
            if (txt_mesh == null)
            {
                txt_mesh = GetComponent<TextMesh>();
                Debug.LogWarning("replaced txt ref with " + txt_mesh, gameObject);
            }
        }
    }


}