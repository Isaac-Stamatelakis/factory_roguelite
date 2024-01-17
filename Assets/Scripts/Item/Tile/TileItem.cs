using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileItemOption {
    Rotation,
    Hardness
}

abstract public class TileItem : ItemObject
{
    public Sprite sprite;
    public List<TileItemOptionValue> options;
}

[System.Serializable]
public class TileItemOptionValue {
    public TileItemOption option;
    public int value;
}
