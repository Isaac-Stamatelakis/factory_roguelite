using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Item.Slot;
using Items;
using Items.Tags;
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
                text = AddTagText(text, itemSlot.tags.Dict);
            }
            ShowToolTip(position, text);
        }

        private static string AddTagText(string text, Dictionary<ItemTag, object> tags)
        {
            if (tags.TryGetValue(ItemTag.CaveData, out var caveData))
            {
                text += $"\nData: <b>{caveData}</b>";
            }

            if (tags.TryGetValue(ItemTag.FluidContainer, out var fluidData))
            {
                ItemSlot fluidItem = fluidData as ItemSlot;
                if (!ItemSlotUtils.IsItemSlotNull(fluidItem))
                {
                    text += $"\nStoring {ItemDisplayUtils.FormatAmountText(fluidItem.amount,false,ItemState.Fluid)} of {fluidItem.itemObject.name} ";
                }
            }
            return text;
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

