using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileValueDecorator : TileValue
{
    public TileValueDecorator (TileValue tileValue) : base(tileValue.Position,tileValue.Id) {

    }
}
