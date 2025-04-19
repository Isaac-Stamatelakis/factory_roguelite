using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using UnityEngine.Tilemaps;

public interface IPlacableItem : ISolidItem
{
    public TileBase getTile();
}
