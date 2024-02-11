using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AInventoryUI : MonoBehaviour {
    protected List<GameObject> slots = new List<GameObject>();
    protected List<ItemSlot> inventory;
    protected void initalizeSlots() {
        for (int n = 0; n < inventory.Count; n ++) {
            initSlot(n);
            loadItem(n);
        }
    }

    protected void refreshSlots() {
        if (slots == null || inventory == null) {
            return;
        }
        for (int n = 0; n < inventory.Count; n ++) {
            GameObject slot = slots[n];
            if (inventory[n] == null || inventory[n].itemObject == null) {
                unloadItem(n);
            }
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
    }
    public virtual void loadItem(int n) {
        GameObject slot = slots[n];
        ItemSlot itemSlot = inventory[n];
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
        if (number == null || item == null) {
            return;
        }
        Destroy(number);
        Destroy(item);
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
    protected virtual GameObject loadItemImage(GameObject slot, ItemSlot data) {
        if (data == null || data.itemObject == null) {
            return null;    
        }
        GameObject imageObject = new GameObject();
        imageObject.name = "item";
        imageObject.transform.SetParent(slot.transform);
        RectTransform rectTransform = imageObject.AddComponent<RectTransform>();
        rectTransform.localPosition = Vector3.zero;
        imageObject.AddComponent<CanvasRenderer>();
        Image image = imageObject.AddComponent<Image>();
        image.sprite = data.itemObject.getSprite();
        rectTransform.sizeDelta = getItemSize(image.sprite);
        return imageObject;
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
    protected virtual GameObject loadItemAmountNumber(GameObject slot, ItemSlot data) {
        if (data == null || data.itemObject == null) {
            return null;    
        }
        GameObject number = new GameObject();
        number.name = "amount";
        number.transform.SetParent(slot.transform);
        number.AddComponent<RectTransform>();
        TextMeshProUGUI textMeshPro = number.AddComponent<TextMeshProUGUI>();
        textMeshPro.text = data.amount.ToString();
        
        
        textMeshPro.fontSize = 30;
        RectTransform rectTransform = textMeshPro.GetComponent<RectTransform>();
        
        rectTransform.localPosition = new Vector3(5f,5f,1);
        rectTransform.sizeDelta = new Vector2(96,96);
        textMeshPro.alignment = TextAlignmentOptions.BottomLeft;
        return number;
    }
    
    public virtual void swapWithGrabbedItem(int n) {
        GameObject grabbedItem = GameObject.Find("GrabbedItem");
        if (grabbedItem == null) {
            Debug.LogError("Inventory " + name + " GrabbedItem is null");
        }
        GrabbedItemProperties grabbedItemProperties = grabbedItem.GetComponent<GrabbedItemProperties>();
        ItemSlot temp = inventory[n];
        inventory[n] = grabbedItemProperties.itemSlot;
        grabbedItemProperties.itemSlot = temp;
        unloadItem(n);
        loadItem(n);
        grabbedItemProperties.updateSprite();
    }

    public static Vector2 getItemSize(Sprite sprite) {
        if (sprite == null) {
            return Vector2.zero;
        }
        Vector2 adjustedSpriteSize = sprite.bounds.size/0.5f;
        if (adjustedSpriteSize.x == 1 && adjustedSpriteSize.y == 1) {
            return new Vector2(32,32);
        }
        if (adjustedSpriteSize.x == adjustedSpriteSize.y) {
            return new Vector2(64,64);
        }
        if (adjustedSpriteSize.x > adjustedSpriteSize.y) {
            return new Vector2(64,adjustedSpriteSize.y/adjustedSpriteSize.x*64);
        }
        if (adjustedSpriteSize.y > adjustedSpriteSize.x) {
            return new Vector2(adjustedSpriteSize.x/adjustedSpriteSize.y*64,64);
        }
        return Vector2.zero;
    }
}

public class InventoryGrid : AInventoryUI
{
    [SerializeField] protected UnityEngine.Vector2Int size;
    public UnityEngine.Vector2Int Size {get{return size;}}
    public int SizeInt{get{return size.x*size.y;}}

    public void initalize(List<ItemSlot> inventory, UnityEngine.Vector2Int size) {
        this.inventory = inventory;
        this.size = size;
        initalizeSlots();
        
    }

    public void initalize(List<ItemSlot> inventory) {
        this.inventory = inventory;
        initalizeSlots();
    }
}
