using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NonGridInventory : AInventoryUI
{
    public void initalize(List<ItemSlot> items) {
        this.inventory = items;
        initalizeSlots();
    }
    public void FixedUpdate() {
        refreshSlots();
    }
}

