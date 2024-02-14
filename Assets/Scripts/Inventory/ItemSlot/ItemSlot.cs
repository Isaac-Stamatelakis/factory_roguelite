using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MatterState {
    Solid,
    NonSolid
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
    public MatterState getState() {
        if (itemObject is SolidItem) {
            return MatterState.Solid;
        }
        if (itemObject is TransmutableItemObject) {
            TransmutableItemState state = ((TransmutableItemObject) itemObject).state;
            return state.getMatterState();
        }
        if (itemObject is NonSolidItem) {
            return MatterState.NonSolid;
        }
        return MatterState.Solid;
    }
}

