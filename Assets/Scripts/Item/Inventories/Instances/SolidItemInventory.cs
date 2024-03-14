using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public interface ILoadableInventory {
    public void initalize(List<ItemSlot> items);
}
public class SolidItemInventory : InventoryUI, ILoadableInventory
{
    public void initalize(List<ItemSlot> items) {
        this.inventory = items;
        initalizeSlots();
    }
    public void FixedUpdate() {
        refreshSlots();
    }
}

