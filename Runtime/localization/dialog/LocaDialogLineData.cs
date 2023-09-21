using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.localization
{
    [System.Serializable]
    public class LocaDialogLineData
    {
        [Tooltip("matching id in trad file text")]
        public LocaDialogLineId lineId;

        [Header("params")]

        //déduit auto pour les checkpoints
        [Tooltip("is using bubble")]
        public bool usePhylactere = true;

        [Tooltip("name of talking entity (for head sprite) ; laisser vide pour utiliser celui du filter")]
        //public DialogCharacters whoIsTalking = DialogCharacters.None;
        [SerializeField]
        public string bubblePivotName = default;

        [Tooltip("la tete du mec qui parle, duh ; laisser vide pour utiliser celui du filter")]
        public Sprite talkerHead;

        [Tooltip("will auto skip after N secondes")]
        public float autoSkipTime = 4f;

        //on avait un skip pour lle timing out mais paul a décider que c'était deprecated

        public LocaDialogLineData(string lineId)
        {
            this.lineId = new LocaDialogLineId(lineId);
            //this.lineId = lineId;
        }

        /// <summary>
        /// si on a ref une durée dans l'editeur
        /// </summary>
        public bool hasAutoSkipSetup() => autoSkipTime > 0f;

        public void debugUpdateCached() => lineId.debugUpdatePreview();
    }

}
