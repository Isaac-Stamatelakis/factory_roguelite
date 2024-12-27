using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items.Transmutable;
using Items;
using Items.Tags;

public enum ItemState {
    Solid,
    Fluid
}

public static class ItemStateExtension {
    public static Color getSlotColor(this ItemState itemState) {
        switch (itemState) {
            case ItemState.Solid:
                return ItemDisplayUtils.SolidItemPanelColor;
            case ItemState.Fluid:
                return ItemDisplayUtils.FluidItemPanelColor;
        }
        return ItemDisplayUtils.SolidItemPanelColor;
    }
}
public interface SolidItem {

}
public interface NonSolidItem {

}
[System.Serializable]
public class ItemSlot
{
    public ItemSlot(ItemObject itemObject, uint amount, ItemTagCollection tags) {
        this.itemObject = itemObject;
        this.amount = amount;
        this.tags = tags;
    }
    public ItemObject itemObject;
    public uint amount;
    public ItemTagCollection tags;
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

