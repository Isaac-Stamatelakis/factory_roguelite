using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum ConduitType {
    Item,
    Fluid,
    Energy,
    Signal
}

public enum ResourceConduitType {
    Item = ConduitType.Item,
    Fluid = ConduitType.Fluid,
    Energy = ConduitType.Energy
}

public abstract class ConduitItem : ItemObject, IPlacableTile
{
    public RuleTile ruleTile;
    public int hardness;
    public override Sprite getSprite()
    {
        return ruleTile.m_DefaultSprite;
    }

    public TileBase getTile()
    {
        return ruleTile;
    }

    public abstract ConduitType getType();
}


