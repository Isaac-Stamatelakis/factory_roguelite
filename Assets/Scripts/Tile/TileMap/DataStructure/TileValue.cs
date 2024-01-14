using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// represents a tile in a tilegrid
/// </summary>
public class TileValue
{
    public TileValue(Vector3Int position, int id) {
        this.placedTile = new PlacedTile(position,Global.getSpriteSize(id));
        this.id = id;
    }
    protected PlacedTile placedTile;
    public PlacedTile PlacedTile {get{return placedTile;} set{placedTile=value;}}
    public Vector3Int Position {get{return placedTile.Position;}}
    public IntervalVector CoveredArea {get{return PlacedTile.getCoveredArea();}}
    protected int id;
    public int Id {get{return id;} set{id=value;}}
    protected SDictionary tileOptions;
    public SDictionary TileOptions {get{return tileOptions;} set{tileOptions=value;}}

}
