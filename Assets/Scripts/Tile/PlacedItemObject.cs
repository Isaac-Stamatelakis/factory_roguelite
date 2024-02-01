using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IPlacedItemObject{
    public abstract ItemObject getItemObject();
}

public abstract class PlacedItemObject<T> : IPlacedItemObject where T : ItemObject
{
    public PlacedItemObject(T itemObject) {
        this.itemObject = itemObject;
    }
    protected T itemObject;

    public override ItemObject getItemObject()
    {
        return this.itemObject;
    }
}
