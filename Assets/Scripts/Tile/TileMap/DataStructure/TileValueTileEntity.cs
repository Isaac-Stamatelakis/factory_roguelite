using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileValueTileEntity : TileValueDecorator
{
    protected GameObject tileEntity;
    public GameObject TileEntity {get{return tileEntity;} set{tileEntity=value;}}
    public TileValueTileEntity (TileValue tileValue, GameObject tileEntity) : base(tileValue) {
        this.tileEntity = tileEntity;
    }
}
