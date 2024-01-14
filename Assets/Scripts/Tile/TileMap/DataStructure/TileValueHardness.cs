using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileValueHardness : TileValueDecorator
{
    protected int hardness;
    public int Harndess {get{return hardness;}}
    public TileValueHardness(TileValue tileValue, int hardness) : base(tileValue) {
        this.hardness = hardness;
    }

    public bool hitTile() {
        hardness--;
        if (hardness == 0) {
            return true;
        }
        return false;
    }    
}
