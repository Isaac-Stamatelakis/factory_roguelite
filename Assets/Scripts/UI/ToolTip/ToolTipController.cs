using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Item.Slot;
using Items;
using Items.Tags;
using Items.Transmutable;
using TileEntity;
using TileEntity.Instances.CompactMachine;
using TileEntity.Instances.CompactMachines;
using UnityEngine;
using UnityEngine.InputSystem;
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
        [SerializeField] private Canvas mWorldCanvas;
        private static ToolTipController instance;
        private Color defaultToolTipColor;
        private Color defaultWorldToolTipColor;
        private ToolTipType toolTipType;
        private Camera canvasCamera;
        private ToolTipUI currentToolTip;

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
            if (itemSlot.itemObject is ITransmutableItem transmutableItem)
            {
                TransmutableItemMaterial material = transmutableItem.getMaterial();
                text += $"\n[<b>{TransmutableItemUtils.FormatChemicalFormula(material.chemicalFormula)}</b>]";
            }

            if (Keyboard.current.shiftKey.isPressed)
            {
                Tier tier = itemSlot.itemObject.GetTier();
                text +=  $"\nTier:<b>{tier}</b>";
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
            if (toolTipType == ToolTipType.World)
            {
                currentToolTip.setText(text);
                currentToolTip.transform.position = position;
                return;
            }
            HideToolTip();
            ToolTipUI newToolTip = GameObject.Instantiate(worldToolTipPrefab, mWorldCanvas.transform, false);
            currentToolTip = newToolTip;
            newToolTip.transform.position = position;
            newToolTip.setText(text);
            
            newToolTip.GetComponent<Image>().color = backGroundColor ?? defaultWorldToolTipColor;
            toolTipType = ToolTipType.World;
            
        }

        
        public void ShowToolTip(Vector2 position, string text, bool useOffset = true, Color? backGroundColor = null) {
            if (toolTipType == ToolTipType.UI)
            {
                currentToolTip.setText(text);
                SetToolTipPosition(currentToolTip.transform as RectTransform);
                return;
            }
            
            HideToolTip();
            ToolTipUI newToolTip = GameObject.Instantiate(toolTipPrefab, transform, false);
            currentToolTip = newToolTip;
            newToolTip.setText(text);   
            
            newToolTip.GetComponent<Image>().color = backGroundColor ?? defaultToolTipColor;
            
            RectTransform rectTransform  = newToolTip.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            SetPivot(rectTransform);
            SetToolTipPosition(rectTransform);
            
            toolTipType = ToolTipType.UI;

            return;

            void SetPivot(RectTransform toolTipTransform)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    toolTipTransform.parent as RectTransform,
                    canvasCamera.WorldToScreenPoint(position),
                    canvasCamera,
                    out Vector2 localPos
                );
                
                Vector3 rightEdgeWorldPos = toolTipTransform.TransformPoint(
                    new Vector3(localPos.x + toolTipTransform.sizeDelta.x, 0, 0)
                );
                Vector3 rightEdgeViewport = canvasCamera.WorldToViewportPoint(rightEdgeWorldPos);
                bool reverse = rightEdgeViewport.x > 0.4f; // Choose a value a bit less than 0.5f
                if (reverse)
                {
                    Vector2 pivot = toolTipTransform.pivot;
                    pivot.x = 1;
                    toolTipTransform.pivot = pivot;
                }
            }
            void SetToolTipPosition(RectTransform toolTipTransform)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    toolTipTransform.parent as RectTransform,
                    canvasCamera.WorldToScreenPoint(position),
                    canvasCamera,
                    out Vector2 localPos
                );
                Vector2 pivot = toolTipTransform.pivot;
                toolTipTransform.anchoredPosition = localPos+ (useOffset ? (Mathf.Approximately(pivot.x, 1) ? -offset : offset) : Vector2.zero);
            }
        }
        
        public void HideToolTip()
        {
            if (!currentToolTip) return;
            GameObject.Destroy(currentToolTip.gameObject);
            toolTipType = ToolTipType.None;
        }
        
        public void HideToolTip(ToolTipType hideType)
        {
            if (toolTipType != hideType) return;
            HideToolTip();
        }
    }
}

