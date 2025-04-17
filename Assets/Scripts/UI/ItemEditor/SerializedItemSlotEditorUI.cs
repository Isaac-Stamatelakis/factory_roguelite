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
            mItemSearchUI.Initialize(parameters.RestrictedItems, OnSelect);
            return;
            void OnSelect(ItemObject itemObject)
            {
                serializedItemSlot.id = itemObject?.id;
                callback?.Invoke(serializedItemSlot);
            }

        }

        public void InitializeList(List<SerializedItemSlot> itemSlots, int index, SerializedItemSlotEditorParameters parameters)
        {
            mItemSearchUI.Initialize(parameters.RestrictedItems, OnSelect);
            return;
            void OnSelect(ItemObject itemObject)
            {
                SerializedItemSlot newSlot = new SerializedItemSlot(
                    itemObject?.id,
                    itemSlots[index]?.amount ?? 1,
                    itemSlots[index]?.tags
                );
                itemSlots[index] = newSlot;
                callback?.Invoke(newSlot);
            }
        }

        
    }

    public class SerializedItemSlotEditorParameters
    {
        public List<ItemObject> RestrictedItems;
        public bool EnableDelete;
        public bool EnableArrows;
        public bool DisplayTags;
        public bool DisplayAmount;
        public Action<SerializedItemSlot> OnValueChange;
        public Action IndexValueChange;
        public Action ListChangeCallback;
    }
    
}

