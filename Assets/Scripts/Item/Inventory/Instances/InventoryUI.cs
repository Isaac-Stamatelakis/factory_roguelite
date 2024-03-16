using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ItemModule;

public abstract class InventoryUI : MonoBehaviour {
    protected List<GameObject> slots = new List<GameObject>();
    protected List<ItemSlot> inventory;
    protected void initalizeSlots() {
        for (int n = 0; n < inventory.Count; n ++) {
            initSlot(n);
            loadItem(n);
        }
    }

    public void FixedUpdate() {
        refreshSlots();
    }

    protected void refreshSlots() {
        if (slots == null || inventory == null) {
            return;
        }
        for (int n = 0; n < slots.Count; n ++) {
            if (inventory[n] == null || inventory[n].itemObject == null) {
                unloadItem(n);
                continue;
            }
            GameObject slot = slots[n];
            reloadItemTag(slot,inventory[n]);
            reloadItemImage(slot,inventory[n]);
            reloadItemAmount(slot,inventory[n]);
        }
    }
    protected virtual void initSlot(int n) {
        Transform slotTransform = transform.Find("slot" + n);
        if (slotTransform == null) {
            Debug.LogError("Slot" + n + " doesn't exist but tried to load it into  inventory " + name);
            slots.Add(null);
            return;
        }
        slots.Add(slotTransform.gameObject);
        initClickHandler(slotTransform,n);
    }

    protected void initClickHandler(Transform slot, int n) {
        
        ItemSlotUIClickHandler clickHandler = slot.GetComponent<ItemSlotUIClickHandler>();
        if (clickHandler == null) {
            Debug.LogError("Slot" + n + " doesn't have click handler");
            return;
        }
        clickHandler.init(this,n);
    }
    public virtual void loadItem(int n) {
        if (n >= slots.Count) {
            return;
        }
        GameObject slot = slots[n];
        ItemSlot itemSlot = inventory[n];
        loadTagVisual(slot,itemSlot); 
        loadItemImage(slot,itemSlot);
        loadItemAmountNumber(slot,itemSlot);  
        
    }

    public virtual void setItem(int n, ItemSlot data) {
        GameObject slot = slots[n];
        if (itemGuard(slot, n,"set item")) {
            return;
        }
        inventory[n]=data;
        loadItem(n);
    }
    public virtual void unloadItem(int n) {
        GameObject slot = slots[n];
        if (slot == null) {
            Debug.LogError("Inventory " + name + "attempted to " + "unload item" + " at slot " + n + " when slot doesn't exist");
            return;
        }
        GameObject number = Global.findChild(slot.transform,"amount");
        GameObject item = Global.findChild(slot.transform,"item");
        GameObject tagObject = Global.findChild(slot.transform,"tags");
        if (number != null) {
            GameObject.Destroy(number);
        }
        if (item != null) {
            GameObject.Destroy(item);
        }
        if (tagObject != null) {
            GameObject.Destroy(tagObject);
        }
    }
    public void updateAmount(int n,int amount) {
        GameObject slot = slots[n];
        if (itemGuard(slot,n,"update amount")) {
            return;
        }
        GameObject number = Global.findChild(slot.transform,"amount");
        if (number == null) {
            return;
        }
        number.GetComponent<TextMeshProUGUI>().text=amount.ToString();
    }

    protected bool itemGuard(GameObject slot, int n, string operation) {
        if (slot == null) {
            Debug.LogError("Inventory " + name + "attempted to " + operation + " at slot " + n + " when slot doesn't exist");
            return true;
        }

        ItemSlot inventorySlot = inventory[n];
        if (inventorySlot == null) {
            return true;
        }
        return false;
    }
    protected virtual GameObject loadItemImage(GameObject slot, ItemSlot itemSlot) {
        return ItemSlotUIFactory.getItemImage(itemSlot,slot.transform);
    }

    protected virtual GameObject loadTagVisual(GameObject slot, ItemSlot itemSlot) {
        return ItemSlotUIFactory.getTagObject(itemSlot,slot.transform);
    }

    protected virtual void reloadItemImage(GameObject slot, ItemSlot data) {
        if (slot == null) {
            return;
        }
        if (data == null || data.itemObject == null) {
            return;   
        }
        Transform imageTransform = slot.transform.Find("item");
        if (imageTransform == null) {
            loadItemImage(slot,data);
            return;
        }
        GameObject imageObject = imageTransform.gameObject;
        Image image = imageObject.GetComponent<Image>();
        image.sprite = data.itemObject.getSprite();
    }

    protected virtual void reloadItemAmount(GameObject slot, ItemSlot data) {
        if (slot == null) {
            return;
        }
        if (data == null || data.itemObject == null) {
            return;   
        }
        Transform numberTransform = slot.transform.Find("amount");
        if (numberTransform == null) {
            loadItemAmountNumber(slot,data);
            return;
        }
        GameObject number = numberTransform.gameObject;
        TextMeshProUGUI textMeshPro = number.GetComponent<TextMeshProUGUI>();
        textMeshPro.text = data.amount.ToString();
    }
    protected virtual void reloadItemTag(GameObject slot, ItemSlot data) {
        if (slot == null) {
            return;
        }
        if (data == null || data.itemObject == null) {
            return;   
        }
        Transform tagTransform = slot.transform.Find("tags");
        if (tagTransform == null) {
            loadTagVisual(slot,data);
        }
        
    }
    protected virtual GameObject loadItemAmountNumber(GameObject slot, ItemSlot data) {
        return ItemSlotUIFactory.getNumber(data,slot.transform);
    }
    public abstract void rightClick(int n);
    public abstract void leftClick(int n);
    public abstract void middleClick(int n);
}
