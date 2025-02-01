using System;
using System.Collections.Generic;
using Conduits.Ports;
using Item.Slot;
using Items;
using Items.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Conduit.Filter
{
    public class ItemFilterUI : MonoBehaviour
    {
        public const string ADDRESSABLE_PATH = "Assets/Prefabs/UI/ConduitPort/FilterEditor.prefab";
        [SerializeField] private Button mToggleRestrictionButton;
        [SerializeField] private TextMeshProUGUI mRestrictionText;
        [SerializeField] private InventoryUI mInventoryUI;
        
        private ItemFilter itemFilter;
        
        public void Initialize(ItemFilter itemFilter, int slotsCount = 9)
        {
            this.itemFilter = itemFilter;
            List<ItemSlot> displaySlots = GetSlots(slotsCount);
            
            mInventoryUI.DisplayInventory(displaySlots);
            mInventoryUI.AddCallback(OnInventoryUpdate);
            mToggleRestrictionButton.onClick.AddListener(() =>
            {
                itemFilter.whitelist = !itemFilter.whitelist;
                Display();
            });
            Display();
        }

        private List<ItemSlot> GetSlots(int size)
        {
            if (itemFilter.ids == null) return ItemSlotUtils.InitEmptyInventory(size);
            List<ItemSlot> slots = new List<ItemSlot>();
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            foreach (string id in itemFilter.ids)
            {
                ItemObject itemObject = itemRegistry.GetItemObject(id);
                if (!itemObject) continue;
                slots.Add(new ItemSlot(itemObject,1,null));
            }

            while (slots.Count < size)
            {
                slots.Add(null);
            }
            while (slots.Count > size)
            {
                slots.RemoveAt(slots.Count-1);
            }

            return slots;
        }

        private void Display()
        {
            mToggleRestrictionButton.GetComponent<Image>().color = itemFilter.whitelist ? 
                new Color(244f/255,243f/255,244f/255,1f) : // Anti-flash white
                new Color(72f/255,72f/255,72f/255,1f); // Dark Grey
            mRestrictionText.text = itemFilter.whitelist ? "White List" : "Black List";
        }
        private void OnInventoryUpdate(int index)
        {
            List<ItemSlot> inventory = mInventoryUI.GetInventory();
            List<string> ids = new List<string>();
            foreach (ItemSlot itemSlot in inventory)
            {
                if (ItemSlotUtils.IsItemSlotNull(itemSlot)) continue;
                string id = itemSlot.itemObject.id;
                if (ids.Contains(id)) continue;
                ids.Add(id);
            }
            itemFilter.ids = ids;
        }
    }
}
