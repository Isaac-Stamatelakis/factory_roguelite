using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSlot
{
    public ItemSlot(ItemObject itemObject, int amount, Dictionary<ItemSlotOption, object> nbt) {
        this.itemObject = itemObject;
        this.amount = amount;
        this.nbt = nbt;
    }
    public ItemObject itemObject;
    public int amount;
    public Dictionary<ItemSlotOption,object> nbt;
}
