using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlacedItemObject<T> where T : ItemObject 
{
    public PlacedItemObject(T itemObject) {
        this.itemObject = itemObject;
    }
    public T itemObject;
}
