using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
Represents data with
**/
public class TileData : PlacedItemObject<TileItem>
{
    public Dictionary<TileItemOption,object> options;

    public TileData(TileItem itemObject, Dictionary<TileItemOption,object> options) : base(itemObject)
    {
        this.options = options;
    }
}
