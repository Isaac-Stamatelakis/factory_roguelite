using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule.Transmutable;
using ItemModule;

public enum ItemState {
    Solid,
    Fluid
}
public interface SolidItem {

}
public interface NonSolidItem {

}
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
    public ItemState getState() {
        if (itemObject is SolidItem) {
            return ItemState.Solid;
        }
        if (itemObject is TransmutableItemObject transmutableItemObject) {
            TransmutableItemState state = transmutableItemObject.getState();
            return state.getMatterState();
        }
        
        if (itemObject is CraftingItem craftingItem) {
            return craftingItem.getItemState();
        }
        if (itemObject is NonSolidItem) {
            return ItemState.Fluid;
        }
        Debug.LogWarning("Get state did not handle state for itemobject " + itemObject.name);
        return ItemState.Solid;
    }
}

