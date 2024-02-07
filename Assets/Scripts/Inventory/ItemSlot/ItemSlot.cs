using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemSlot
{
    public ItemSlot(ItemObject itemObject, int amount, Dictionary<string, object> nbt) {
        this.itemObject = itemObject;
        this.amount = amount;
        this.nbt = nbt;
    }
    public ItemObject itemObject;
    public int amount;
    public Dictionary<string,object> nbt;
}

