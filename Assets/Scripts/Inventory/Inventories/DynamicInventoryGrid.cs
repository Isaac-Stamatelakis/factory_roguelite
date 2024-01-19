using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicInventoryGrid : InventoryGrid
{
    protected override void initSlot(int n)
    {
        GameObject slot = Global.findChild(transform,"slot"+n);
        if (slot == null) {
            addSlot();
        } else {
            slots.Add(slot);
        }
    }
    public bool addSlot() {
        if (slots.Count >= size.x * size.y) {
            return false;
        }
        GameObject slot = Instantiate(Resources.Load<GameObject>("Prefabs/GUI/ItemInventorySlot"));
        slot.name = "slot" + (slots.Count).ToString();
        slot.transform.SetParent(transform,false);
        slots.Add(slot);
        loadItem(slots.Count-1);
        return true;
    }
    public bool popSlot() {
        if (slots.Count <= 0) {
            return false;
        }
        GameObject slot = slots[slots.Count-1];
        slots.RemoveAt(slots.Count-1);
        Destroy(slot);
        return true;
    }

    public void updateSize(Vector2Int newSize) {
        this.size = newSize;
        while (slots.Count < SizeInt) {
            addSlot();
        }
        while (slots.Count > SizeInt) {
            popSlot();
        }
    }
}
