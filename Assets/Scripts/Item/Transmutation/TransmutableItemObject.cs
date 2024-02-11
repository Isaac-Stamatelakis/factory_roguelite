using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransmutableItemObject : ItemObject
{
    public TransmutableItemState state;
    public TransmutableMaterialDict materialDict;
    public TransmutableItemMaterial material;
    public Sprite sprite;

    public override Sprite getSprite()
    {
        return this.sprite;
    }
}
