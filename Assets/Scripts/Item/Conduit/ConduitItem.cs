using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

public abstract class ConduitItem : ItemObject
{
    public RuleTile ruleTile;
    public int hardness;
    public override Sprite getSprite()
    {
        return ruleTile.m_DefaultSprite;
    }
    public abstract ConduitType getType();
}


