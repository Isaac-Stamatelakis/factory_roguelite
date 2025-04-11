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
        private Camera canvasCamera;

        public static ToolTipController Instance { get => instance; }

        public void Awake() {
            instance = this;
            defaultToolTipColor = toolTipPrefab.GetComponent<Image>().color;
            defaultWorldToolTipColor = worldToolTipPrefab.GetComponent<Image>().color;
            canvasCamera = GetComponentInParent<Canvas>().worldCamera;
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
            
            newToolTip.GetComponent<Image>().color = backGroundColor ?? defaultWorldToolTipColor;
            toolTipType = ToolTipType.World;
            SetToolTipPosition(position, newToolTip.transform);
        }

        private void SetToolTipPosition(Vector2 position, Transform toolTipTransform)
        {
            Canvas canvas = transform.parent.GetComponent<Canvas>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                position,
                canvas.worldCamera, // Use 'null' for Overlay Canvas
                out Vector2 localPoint
            );
            RectTransform rectTransform = (RectTransform)toolTipTransform;
            rectTransform.anchoredPosition = localPoint;
        }
        public void ShowToolTip(Vector2 position, string text, bool useOffset = true, Color? backGroundColor = null, bool reverse = false) {
            HideToolTip();
            ToolTipUI newToolTip = GameObject.Instantiate(toolTipPrefab, transform, false);
            newToolTip.setText(text);   
            
            newToolTip.GetComponent<Image>().color = backGroundColor ?? defaultToolTipColor;
            RectTransform rectTransform = (RectTransform)newToolTip.transform;
            if (reverse)
            {
                var pivot = rectTransform.pivot;
                pivot.x = 1;
                rectTransform.pivot = pivot;
            }
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform.parent as RectTransform,
                Input.mousePosition,
                canvasCamera,
                out Vector2 localPos
            );
            rectTransform.anchoredPosition = localPos+ (useOffset ? (reverse ? -offset : offset) : Vector2.zero);
            toolTipType = ToolTipType.UI;
        }
        
        public void HideToolTip()
        {
            if (transform.childCount == 0) return;
            GlobalHelper.DeleteAllChildren(transform);
            toolTipType = ToolTipType.None;
        }
        
        public void HideToolTip(ToolTipType hideType)
        {
            if (toolTipType != hideType) return;
            HideToolTip();
        }
    }
}

