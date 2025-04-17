using System.Collections.Generic;
using DevTools.CraftingTrees.Network;
using Item.Slot;
using Items;
using Items.Inventory;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DevTools.CraftingTrees.TreeEditor.NodeEditors
{
    internal class CraftingNodeItemEditorUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI mTitleText;
        [SerializeField] private InventoryUI mInventoryUI;
        [SerializeField] private SerializedItemSlotEditorUI mItemSlotEditorUI;

        public void Display(ItemNodeData itemNodeData, CraftingTreeGeneratorUI generatorUI)
        {
            mInventoryUI.SetInteractMode(InventoryInteractMode.OverrideAction);
            mInventoryUI.OverrideClickAction(ClickOverride);
            DisplayInventory();
            return;

            void ClickOverride(PointerEventData.InputButton inputButton, int index)
            {
                SerializedItemSlotEditorUI serializedItemSlotEditorUI = GameObject.Instantiate(mItemSlotEditorUI);
                SerializedItemSlotEditorParameters parameters = new SerializedItemSlotEditorParameters
                {
                    OnValueChange = OnItemChange
                };
                serializedItemSlotEditorUI.Initialize(itemNodeData.SerializedItemSlot,parameters);
                
                CanvasController.Instance.DisplayObject(serializedItemSlotEditorUI.gameObject,hideParent:false);
            }

            void OnItemChange(SerializedItemSlot serializedItemSlot)
            {
                itemNodeData.SerializedItemSlot = serializedItemSlot;
                DisplayInventory();
                generatorUI.Rebuild();
            }

            void DisplayInventory()
            {
                ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(itemNodeData.SerializedItemSlot);
                mTitleText.text = ItemSlotUtils.IsItemSlotNull(itemSlot) ? "Null" : itemSlot.itemObject.name;
                mInventoryUI.DisplayInventory(new List<ItemSlot>{itemSlot},clear:false);
            }
        }
        
    }
}
