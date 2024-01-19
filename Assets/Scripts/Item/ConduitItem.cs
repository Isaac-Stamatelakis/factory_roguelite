using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum ConduitType {
    Item,
    Fluid,
    Energy,
    Signal
}

public class ConduitItem : ItemObject
{
    public ConduitType type;
    public RuleTile ruleTile;
    public int hardness;
    public Color color;
    public int speed;
    public ConduitOptions conduitOptions;
}
