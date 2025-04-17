using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Items;
using Items.Inventory;
using System;
using Item.Slot;
using UI.Catalogue.ItemSearch;
using UI.ItemEditor;

namespace UI {
    public class SerializedItemSlotEditorUI : MonoBehaviour
    {
        [SerializeField] private ItemSearchUI mItemSearchUI;
        [SerializeField] private SlotValueEditorUI mSlotValueEditorUI;
        private Action<SerializedItemSlot> callback;
        
        public void Initialize(SerializedItemSlot serializedItemSlot, SerializedItemSlotEditorParameters parameters)
        {
            mItemSearchUI.Initialize(parameters.RestrictedItems,OnSelect);
            
            return;

            void OnSelect(ItemObject itemObject)
            {
                if (parameters.ItemSlots != null)
                {
                    List<SerializedItemSlot> itemSlots = parameters.ItemSlots;
                    SerializedItemSlot newSlot = new SerializedItemSlot(
                        itemObject?.id,
                        itemSlots[parameters.Index]?.amount ?? 1,
                        itemSlots[parameters.Index]?.tags
                    );
                    itemSlots[parameters.Index] = newSlot;
                }
                else
                {
                    serializedItemSlot.id = itemObject?.id;
                }
                
                callback?.Invoke(serializedItemSlot);
            }
        }
    }
    public class SerializedItemSlotEditorParameters
    {
        public List<ItemObject> RestrictedItems;
        public List<SerializedItemSlot> ItemSlots;
        
        public int Index;
        public bool EnableDelete;
        public bool EnableArrows;
        public bool DisplayTags;
        public bool DisplayAmount;
        public Action<SerializedItemSlot> OnItemChange;
        public Action ChangeCallback;
        public Action ListChangeCallback;
    }
    
}

