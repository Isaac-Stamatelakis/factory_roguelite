using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryGrid : MonoBehaviour
{
    private static string TAG = "InventoryGrid: ";
    protected List<GameObject> slots = new List<GameObject>();
    protected List<Dictionary<string,object>> inventory;
    [SerializeField] protected Vector2Int size;
    public Vector2Int Size {get{return size;}}
    public int SizeInt{get{return size.x*size.y;}}

    public void initalize(List<Dictionary<string,object>> inventory, Vector2Int size) {
        this.inventory = inventory;
        this.size = size;
        initalizeSlots();
        
    }

    public void initalize(List<Dictionary<string,object>> inventory) {
        this.inventory = inventory;
        initalizeSlots();
    }

    protected void initalizeSlots() {
        for (int n = 0; n < SizeInt; n ++) {
            initSlot(n);
            loadItem(n);
        }
    }
    protected virtual void initSlot(int n) {
        GameObject slot = Global.findChild(transform,"slot"+n);
        slots.Add(slot);
    }
    public virtual void loadItem(int n) {
        GameObject slot = slots[n];

        if (itemGuard(slot, n,"load item")) {
            return;
        }
        
        if (Global.findChild(slot.transform,"item") != null) {
            return;
        } 

        if (Global.findChild(slot.transform,"amount") != null) {
            return;
        }

        Dictionary<string,object> data = inventory[n];
        /*
        if (!validateData(data)) {
            return;
        }
        */
        loadItemImage(slot,data);
        loadItemAmountNumber(slot,data);   
        
    }

    /*
    protected virtual bool validateData(Dictionary<string,object> data) {
        return false;
    }
    */

    public virtual void setItem(int n, Dictionary<string,object> data) {
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
            Debug.LogError(TAG + "attempted to " + "unload item" + " at slot " + n + " when slot doesn't exist");
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
            Debug.LogError(TAG + "attempted to " + operation + " at slot " + n + " when slot doesn't exist");
            return true;
        }

        Dictionary<string,object> data = inventory[n];
        if (data == null) {
            return true;
        }
        return false;
    }
    protected virtual GameObject loadItemImage(GameObject slot, Dictionary<string,object> data) {
        GameObject imageObject = new GameObject();
        imageObject.name = "item";
        imageObject.transform.SetParent(slot.transform);
        RectTransform rectTransform = imageObject.AddComponent<RectTransform>();
        rectTransform.localPosition = Vector3.zero;
        return imageObject;
    }

    protected virtual GameObject loadItemAmountNumber(GameObject slot, Dictionary<string,object> data) {
        GameObject number = new GameObject();
        number.name = "amount";
        number.transform.SetParent(slot.transform);
        number.AddComponent<RectTransform>();
        return number;
    }
    

    public virtual void swapWithGrabbedItem(int n) {
        GameObject grabbedItem = GameObject.Find("GrabbedItem");
        if (grabbedItem == null) {
            Debug.LogError(TAG + "GrabbedItem is null");
        }
        GrabbedItemProperties grabbedItemProperties = grabbedItem.GetComponent<GrabbedItemProperties>();
        Dictionary<string,object> grabbedItemData = grabbedItemProperties.GrabbedItemData;
        grabbedItemProperties.GrabbedItemData = inventory[n];
        inventory[n] = grabbedItemData;
        if (inventory[n] == null || System.Convert.ToInt32(inventory[n]["id"]) == -1) {
            unloadItem(n);
        } else {
            loadItem(n);
        }
        grabbedItemProperties.updateSprite();
    }

    public static Vector2 getItemSize(Sprite sprite) {
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
