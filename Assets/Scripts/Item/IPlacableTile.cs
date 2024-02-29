using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public interface IPlacableItem : SolidItem
{
    public TileBase getTile();
}
