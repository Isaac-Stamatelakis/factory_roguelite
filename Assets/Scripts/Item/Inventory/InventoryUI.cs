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
using TileEntity;
using UnityEngine.EventSystems;

namespace Items.Inventory {
    public enum InventoryInteractMode
    {
        Standard,
        Recipe,
        UnInteractable,
        BlockInput,
        OverrideAction
        
    }
    public interface IIndexInventoryListener
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
        protected List<IIndexInventoryListener> listeners = new List<IIndexInventoryListener>();
        protected List<Action<int>> callbacks = new List<Action<int>>();
        protected List<Action> genericCallbacks = new List<Action>();
        protected Action<int> onHighlight;
        protected Func<int, string> toolTipOverride;
        public Func<int, string> ToolTipOverride => toolTipOverride;
        private int highlightedSlot = -1;
        private bool refresh;
        public InventoryInteractMode InventoryInteractMode = InventoryInteractMode.Standard;
        private InventoryUI connection;
        private HashSet<ItemTag> tagRestrictions = new HashSet<ItemTag>();
        public InventoryUI Connection => connection;
        private InventoryRestrictionMode restrictionMode;
        private string restrictedItemId;
        public InventoryRestrictionMode TagRestrictionMode => restrictionMode;
        [SerializeField] private bool enableToolTip = true;
        public bool EnableToolTip => enableToolTip;
        private uint maxStackSize = Global.MAX_SIZE;
        public uint MaxSize => maxStackSize;
        private Action<PointerEventData.InputButton, int> overrideClickAction;
        private Func<ItemObject, int, bool> validateInputCallback;
        private Color highlightColor = new Color(255 / 255f, 215 / 255f, 0, 100 / 255f);
        private Color defaultItemPanelColor;
        
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

            if (itemSlotUIPrefab)
            {
                defaultItemPanelColor = itemSlotUIPrefab.GetComponent<Image>().color;
            }
            else
            {
                if (transform.childCount > 0) defaultItemPanelColor = transform.GetChild(0).gameObject.GetComponent<Image>().color;
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
        
        public void DisplayBottomText(List<string> bottomText)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (i >= slots.Count || i >= bottomText.Count) break;
                slots[i].DisplayBottomText(bottomText[i]);
            }
        }

        public void SetHighlightColor(Color color)
        {
            highlightColor = color;
            DisplayHighSlotSlot(highlightedSlot);
        }

        public void SetOnHighlight(Action<int> onHighlight)
        {
            this.onHighlight = onHighlight;
        }
        public void SetAllPanelColors(Color color)
        {
            defaultItemPanelColor = color;
            foreach (ItemSlotUI slotUI in slots)
            {
                if (!slotUI.mTopText) continue;
                slotUI.Panel.color = color;
            }
        }
        
        public void SetAllTopTextColors(Color color)
        {
            defaultItemPanelColor = color;
            foreach (ItemSlotUI slotUI in slots)
            {
                if (!slotUI.mTopText) continue;
                slotUI.mTopText.color = color;
            }
        }

        public void ApplyFunctionToAllSlots(Action<ItemSlotUI> action)
        {
            foreach (ItemSlotUI slotUI in slots)
            {
                action.Invoke(slotUI);
            }
        }

        public void OverrideClickAction(Action<PointerEventData.InputButton,int> callback)
        {
            InventoryInteractMode = InventoryInteractMode.OverrideAction;
            overrideClickAction = callback;
        }

        public Action<PointerEventData.InputButton, int> GetOverrideAction()
        {
            return overrideClickAction;
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
                GlobalHelper.DeleteAllChildren(transform);
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
        
        
        public void AddListener(IIndexInventoryListener listener) {
            listeners.Add(listener);
        }

        public void AddCallback(Action<int> callback)
        {
            callbacks.Add(callback);
        }
        
        public void AddCallback(Action callback)
        {
            genericCallbacks.Add(callback);
        }
        
        

        public void CallListeners(int index)
        {
            foreach (IIndexInventoryListener listener in listeners)
            {
                listener.InventoryUpdate(index);
            }

            foreach (Action<int> callback in callbacks)
            {
                callback?.Invoke(index);
            }
            foreach (Action callback in genericCallbacks)
            {
                callback?.Invoke();
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

        public void SetMaxSize(uint max)
        {
            Debug.Log(max);
            maxStackSize = max;
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
        
        public ItemSlotUI GetItemSlotUI(int index) {
            if (index < 0 || index >= inventory?.Count) {
                return null;
            }
            return slots[index];
        }
        
        public void RefreshSlots() {
            if (slots == null || inventory == null) {
                return;
            }
            for (int n = 0; n < slots.Count; n ++) {
                RefreshSlot(n);
            }
        }
        
        public void RefreshSlot(int n) {
            if (n < 0 || n >= slots.Count || n >= inventory.Count) {
                return;
            }
            slots[n].Display(inventory[n]);
        }
        
        protected void InitClickHandler(Transform slot, int n) {
            ItemSlotUIClickHandler clickHandler = slot.GetComponent<ItemSlotUIClickHandler>();
            clickHandler?.Initialize(this,n);
            
        }
        
        public void SetItem(int n, ItemSlot data) {
            if (n < 0 || n >= slots.Count) {
                return;
            }
            inventory[n]=data;
            RefreshSlot(n);
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
            if (n < 0 || n >= slots.Count) return;
            if (highlightedSlot >= 0 && n < slots.Count)
            {
                slots[highlightedSlot].GetComponent<Image>().color = defaultItemPanelColor;
            }
            
            highlightedSlot = n;
            slots[highlightedSlot].GetComponent<Image>().color = highlightColor;
            onHighlight?.Invoke(n);
        }

        public void AddTagRestriction(ItemTag itemTag)
        {
            tagRestrictions.Add(itemTag);
        }

        public void SetItemRestriction(ItemObject itemObject)
        {
            restrictedItemId = itemObject?.id;
        }

        public void RemoveRestriction(ItemTag itemTag)
        {
            tagRestrictions.Remove(itemTag);
        }

        public void SetRestrictionMode(InventoryRestrictionMode inventoryRestrictionMode)
        {
            this.restrictionMode = inventoryRestrictionMode;
        }

        public void SetToolTipOverride(Func<int, string> toolTipOverride)
        {
            this.toolTipOverride = toolTipOverride;
        }

        public bool ValidateInput(ItemSlot itemSlot, int inputIndex)
        {
            // Many different options for this :)
            if (InventoryInteractMode == InventoryInteractMode.BlockInput) return false;
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) return false;
            if (restrictedItemId != null && itemSlot.itemObject.id != restrictedItemId) return false;
            if (validateInputCallback != null && !validateInputCallback(itemSlot?.itemObject,inputIndex)) return false;
            if (restrictionMode == InventoryRestrictionMode.None) return true;
            
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

        public void SetInputRestrictionCallBack(Func<ItemObject, int, bool> callback)
        {
            validateInputCallback = callback;
        }

        private bool PassWhiteList(ItemSlot itemSlot)
        {
            
            return itemSlot.itemObject.CanApplyTags(tagRestrictions) || (itemSlot.tags != null && itemSlot.tags.HasTags(tagRestrictions));
        }
        
        
        
    }

}

