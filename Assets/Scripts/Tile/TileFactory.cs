using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class TileFactory
{
    public static TileBase generateTile(TileData tileData) {
        TileBase tile = tileData.itemObject.tile;
        Dictionary<TileItemOption, object> options = tileData.options;
        int chisel = 0;
        if (options.ContainsKey(TileItemOption.Chisel)) {
            chisel = Convert.ToInt32(options[TileItemOption.Chisel]);
        }
        if (tile is RotatableTile) {
            int rotation = 0;
            if (options.ContainsKey(TileItemOption.Rotation)) {
                rotation = Convert.ToInt32(options[TileItemOption.Rotation]);
            }
        }
        return tile;
    }

}
