using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Item.Slot;
using Items;
using Items.Tags;
using Items.Transmutable;
using TileEntity.Instances.CompactMachine;
using TileEntity.Instances.CompactMachines;
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

        public void ShowToolTip(Vector2 position, ItemSlot itemSlot)
        {
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) return;
            string text = itemSlot.itemObject.name;
            if (itemSlot.itemObject is TransmutableItemObject transmutableItemObject)
            {
                TransmutableItemMaterial material = transmutableItemObject.getMaterial();
                text += $"\n[<b>{TransmutableItemUtils.FormatChemicalFormula(material.chemicalFormula)}</b>]";
            }

            if (itemSlot.tags?.Dict != null)
            {
                string totalTagText = "\n";
                foreach (var (itemTag, data) in itemSlot.tags.Dict)
                {
                    string tagText = itemTag.GetTagText(data);
                    if (String.IsNullOrEmpty(tagText)) continue;
                    totalTagText += tagText;
                }
                if (totalTagText.Length > 1) text += totalTagText;
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

