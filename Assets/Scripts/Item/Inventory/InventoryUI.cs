using System;
using System.Collections;
using System.Collections.Generic;
using Item.Display.ClickHandlers;
using Item.Inventory;
using Item.Slot;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items;
using Items.Tags;

namespace Items.Inventory {
    public enum InventoryInteractMode
    {
        Standard,
        Recipe,
        UnInteractable,
        BlockInput
        
    }
    public interface IInventoryListener
    {
        public void InventoryUpdate(int n);
    }

    public enum InventoryRestrictionMode
    {
        None,
        WhiteList,
        BlackList
    }
    public class InventoryUI : MonoBehaviour
    {
        
        [SerializeField] private ItemSlotUI itemSlotUIPrefab;
        protected List<ItemSlotUI> slots = new List<ItemSlotUI>();
        protected List<ItemSlot> inventory;
        protected List<IInventoryListener> listeners = new List<IInventoryListener>();
        private int highlightedSlot = -1;
        private bool refresh;
        public InventoryInteractMode InventoryInteractMode = InventoryInteractMode.Standard;
        private InventoryUI connection;
        private HashSet<ItemTag> tagRestrictions = new HashSet<ItemTag>();
        public InventoryUI Connection => connection;
        private InventoryRestrictionMode restrictionMode;
        public InventoryRestrictionMode TagRestrictionMode => restrictionMode;
        private bool enableToolTip = true;
        public bool EnableToolTip => enableToolTip;
        
        public void Awake()
        {
            List<GameObject> toDestroy = new List<GameObject>();
            for (int i = 0; i < transform.childCount; i++)
            {
                ItemSlotUI itemSlotUI = transform.GetChild(i).GetComponent<ItemSlotUI>();
                if (ReferenceEquals(itemSlotUI,null))
                {
                    toDestroy.Add(transform.gameObject);
                    continue;
                }
                slots.Add(itemSlotUI);
                ItemSlotUIClickHandler clickHandler = itemSlotUI.GetComponent<ItemSlotUIClickHandler>();
                clickHandler?.Initialize(this,slots.Count-1);
            }

            foreach (GameObject go in toDestroy)
            {
                Destroy(go);
            }
        }

        public void DisplayInventory(List<ItemSlot> displayInventory, bool clear = true)
        {
            int displayAmount = displayInventory?.Count ?? 0;
            DisplayInventory(displayInventory, displayAmount, clear:clear);
        }

        public void DisplayTopText(List<string> textList)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (i >= slots.Count || i >= textList.Count) break;
                slots[i].SetTopText(textList[i]);
            }
        }

        public void ResetTopText()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (i >= slots.Count) break;
                slots[i].SetTopText(string.Empty);
            }
        }

        public void SetInteractMode(InventoryInteractMode mode)
        {
            InventoryInteractMode = mode;
        }

        public void SetConnection(InventoryUI connection)
        {
            this.connection = connection;
        }

        public void DisplayInventory(List<ItemSlot> displayInventory, int displayAmount, bool clear = true)
        {
            if (clear)
            {
                slots.Clear();
                GlobalHelper.deleteAllChildren(transform);
            }
            inventory = displayInventory;
            if (inventory == null)
            {
                return;
            }
            while (slots.Count < displayAmount)
            {
                AddSlot();
            }
            while (slots.Count > displayAmount)
            {
                PopSlot();
            }

            for (int i = 0; i < displayAmount; i++)
            {
                slots[i].Display(itemSlot: i < displayInventory.Count ? displayInventory[i] : null);
            }
            if (highlightedSlot >= 0 && highlightedSlot < slots.Count) DisplayHighSlotSlot(highlightedSlot); 
        }
        
        
        public void AddListener(IInventoryListener listener) {
            listeners.Add(listener);
        }

        public void CallListeners(int index)
        {
            foreach (IInventoryListener listener in listeners)
            {
                listener.InventoryUpdate(index);
            }
        }

        public List<ItemSlot> GetInventory()
        {
            return inventory;
        }

        public void SetRefresh(bool refresh)
        {
            this.refresh = refresh;
        }

        public void FixedUpdate()
        {
            if (!refresh) return;
            RefreshSlots();
        }

        public TClickHandler[] GetClickHandlers<TClickHandler>() where TClickHandler : ItemSlotUIClickHandler
        {
            return GetComponentsInChildren<TClickHandler>();
        }
         
        protected void AddSlot() {
            ItemSlotUI slot = Instantiate(itemSlotUIPrefab, transform);
            slots.Add(slot);
            InitClickHandler(slot.transform,slots.Count-1);
        }

        protected void PopSlot() {
            if (slots.Count <= 0) {
                return;
            }
            ItemSlotUI slot = slots[^1]; // last
            slots.RemoveAt(slots.Count-1);
            Destroy(slot.gameObject);
        }
        
        public ItemSlot GetItemSlot(int index) {
            if (index < 0 || index >= inventory?.Count) {
                return null;
            }
            return inventory?[index];
        }
        
        public void RefreshSlots() {
            if (slots == null || inventory == null) {
                return;
            }
            for (int n = 0; n < slots.Count; n ++) {
                DisplayItem(n);
            }
        }
        public void DisplayItem(int n) {
            if (n < 0 || n >= slots.Count || n >= inventory.Count) {
                return;
            }
            slots[n].Display(inventory[n]);
        }
        
        protected void InitClickHandler(Transform slot, int n) {
            ItemSlotUIClickHandler clickHandler = slot.GetComponent<ItemSlotUIClickHandler>();
            clickHandler?.Initialize(this,n);
            
        }
        
        public virtual void SetItem(int n, ItemSlot data) {
            if (n < 0 || n >= slots.Count) {
                return;
            }
            inventory[n]=data;
            DisplayItem(n);
            CallListeners(n);
        }
        
        public void HighlightSlot(int n)
        {
            if (n == highlightedSlot) {
                return;
            }

            DisplayHighSlotSlot(n);
        }

        private void DisplayHighSlotSlot(int n)
        {
            if (highlightedSlot >= 0 && n < slots.Count)
            {
                slots[highlightedSlot].GetComponent<Image>().color = itemSlotUIPrefab.GetComponent<Image>().color;
            }
            
            highlightedSlot = n;
            slots[highlightedSlot].GetComponent<Image>().color = new Color(255/255f,215/255f,0,100/255f);
        }

        public void AddRestriction(ItemTag itemTag)
        {
            tagRestrictions.Add(itemTag);
        }

        public void RemoveRestriction(ItemTag itemTag)
        {
            tagRestrictions.Remove(itemTag);
        }

        public void SetRestrictionMode(InventoryRestrictionMode inventoryRestrictionMode)
        {
            this.restrictionMode = inventoryRestrictionMode;
        }

        public bool ValidateItemSlot(ItemSlot itemSlot)
        {
            if (restrictionMode == InventoryRestrictionMode.None) return true;
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) return false;
            
            switch (restrictionMode)
            {
                case InventoryRestrictionMode.WhiteList:
                    return PassWhiteList(itemSlot);
                case InventoryRestrictionMode.BlackList:
                    return !PassWhiteList(itemSlot);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool PassWhiteList(ItemSlot itemSlot)
        {
            
            return itemSlot.itemObject.CanApplyTags(tagRestrictions) || (itemSlot.tags != null && itemSlot.tags.HasTags(tagRestrictions));
        }
        
        
        
    }

}

