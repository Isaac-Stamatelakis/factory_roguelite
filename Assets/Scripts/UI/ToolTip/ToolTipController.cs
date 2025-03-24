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
using UnityEngine.UI;

namespace UI.ToolTip {
    public enum ToolTipType
    {
        None,
        UI,
        World
    }
    public class ToolTipController : MonoBehaviour
    {
        private readonly Vector2 offset = new Vector2(60, 0);
        [SerializeField] private ToolTipUI toolTipPrefab;
        [SerializeField] private ToolTipUI worldToolTipPrefab;
        private static ToolTipController instance;
        private Color defaultToolTipColor;
        private Color defaultWorldToolTipColor;
        private ToolTipType toolTipType;

        public static ToolTipController Instance { get => instance; }

        public void Awake() {
            instance = this;
            defaultToolTipColor = toolTipPrefab.GetComponent<Image>().color;
            defaultWorldToolTipColor = worldToolTipPrefab.GetComponent<Image>().color;
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

        /// <summary>
        /// Displays a tool tip that is centered at position
        /// </summary>
        public void ShowWorldToolTip(Vector2 position, string text, Color? backGroundColor = null)
        {
            HideToolTip();
            ToolTipUI newToolTip = GameObject.Instantiate(worldToolTipPrefab, transform, false);
            newToolTip.setText(text);
            Vector2 displayPosition = position;
            newToolTip.transform.position = displayPosition;
            newToolTip.GetComponent<Image>().color = backGroundColor ?? defaultWorldToolTipColor;
            toolTipType = ToolTipType.World;
        }
        
        public void ShowToolTip(Vector2 position, string text, bool useOffset = true, Color? backGroundColor = null) {
            HideToolTip();
            ToolTipUI newToolTip = GameObject.Instantiate(toolTipPrefab, transform, false);
            newToolTip.setText(text);   
            Vector2 displayPosition = position + (useOffset ? offset : Vector2.zero);
            newToolTip.transform.position = displayPosition;
            newToolTip.GetComponent<Image>().color = backGroundColor ?? defaultToolTipColor;
            toolTipType = ToolTipType.UI;
        }
        
        public void HideToolTip()
        {
            if (transform.childCount == 0) return;
            GlobalHelper.deleteAllChildren(transform);
            toolTipType = ToolTipType.None;
        }
        
        public void HideToolTip(ToolTipType hideType)
        {
            if (toolTipType != hideType) return;
            HideToolTip();
        }
    }
}

