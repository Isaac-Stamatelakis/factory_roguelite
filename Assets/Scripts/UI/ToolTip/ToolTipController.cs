using System.Collections;
using System.Collections.Generic;
using Items;
using Items.Transmutable;
using UnityEngine;

namespace UI.ToolTip {
    public class ToolTipController : MonoBehaviour
    {
        private readonly Vector2 offset = new Vector2(60, 0);
        [SerializeField] private ToolTipUI toolTipPrefab;
        private static ToolTipController instance;

        public static ToolTipController Instance { get => instance; }

        public void Awake() {
            instance = this;
        }

        public void ShowToolTip(Vector2 position, ItemObject itemObject)
        {
            if (ReferenceEquals(itemObject, null)) return;
            string text = itemObject.name;
            if (itemObject is TransmutableItemObject transmutableItemObject)
            {
                TransmutableItemMaterial material = transmutableItemObject.getMaterial();
                text += '\n' + TransmutableItemUtils.FormatChemicalFormula(material.chemicalFormula);
            }
            ShowToolTip(position, text);
        }
        
        public void ShowToolTip(Vector2 position, string text) {
            HideToolTip();
            ToolTipUI newToolTip = GameObject.Instantiate(toolTipPrefab, transform, false);
            newToolTip.setText(text);
            newToolTip.transform.position = position+offset;
        }
        public void HideToolTip()
        {
            if (transform.childCount == 0) return;
            GlobalHelper.deleteAllChildren(transform);
        }
    }
}

