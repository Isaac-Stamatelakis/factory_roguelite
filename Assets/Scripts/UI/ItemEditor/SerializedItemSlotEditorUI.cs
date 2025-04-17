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

        public void Initialize(SerializedItemSlot serializedItemSlot, Action<SerializedItemSlot> callback, SerializedItemSlotEditorParameters parameters,List<ItemObject> itemRestrictions = null)
        {
            mItemSearchUI.Initialize(itemRestrictions, OnSelect);
            if (parameters == null)
            {
                mSlotValueEditorUI.gameObject.SetActive(false);
            }
            else
            {
                mSlotValueEditorUI.Display(serializedItemSlot,parameters);
            }
            return;
            void OnSelect(ItemObject itemObject)
            {
                serializedItemSlot.id = itemObject?.id;
                callback?.Invoke(serializedItemSlot);
                if (parameters == null) CanvasController.Instance.PopStack();
            }

        }

        public void InitializeList(List<SerializedItemSlot> itemSlots, int index, SerializedItemSlotEditorParameters parameters,List<ItemObject> itemRestrictions = null)
        {
            mItemSearchUI.Initialize(itemRestrictions, OnSelect);
            if (parameters == null)
            {
                mSlotValueEditorUI.gameObject.SetActive(false);
            }
            else
            {
                mSlotValueEditorUI.Display(itemSlots,index,parameters);
            }
            return;
            void OnSelect(ItemObject itemObject)
            {
                SerializedItemSlot newSlot = new SerializedItemSlot(
                    itemObject?.id,
                    itemSlots[index]?.amount ?? 1,
                    itemSlots[index]?.tags
                );
                itemSlots[index] = newSlot;
                parameters.IndexValueChange?.Invoke();
                callback?.Invoke(newSlot);
                mSlotValueEditorUI.Display(itemSlots,index,parameters);
            }
        }
    }
    
    

    public class SerializedItemSlotEditorParameters
    {
        public bool EnableDelete;
        public bool EnableArrows;
        public bool DisplayTags;
        public bool DisplayAmount;
        public Action<SerializedItemSlot> OnValueChange;
        public Action IndexValueChange;
        public Action ListChangeCallback;
    }
    
}

