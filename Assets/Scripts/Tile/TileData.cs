using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiles;

/**
Represents data with
**/
public class TileData : PlacedItemObject<TileItem>
{
    public Dictionary<TileOption,object> options;

    public TileData(TileItem itemObject, Dictionary<TileOption,object> options) : base(itemObject)
    {
        this.options = options;
    }
}
