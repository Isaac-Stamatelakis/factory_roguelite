using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public enum IntTileItemOption {
    Rotation,
    Hardness
}

public enum ScriptableTileItemOption {
    RuleTile,
    AnimatedTile,
}

public enum TileEntityOption {
    Main,
    Visual,
    Conduit
}

public enum TileType {
    Block,
    Background,
    Object
}
public class TileItem : ItemObject
{
    public Sprite sprite;
    [Tooltip("Specify the integer value for given tile options")]
    public List<TileItemOptionValue<IntTileItemOption,int>> integerOptions;
    [Tooltip("Specify the integer value for given tile options\nNote if both RuleTile and AnimatedTile are provided, RuleTile is used")]
    public List<TileItemOptionValue<ScriptableTileItemOption, ScriptableObject>> scriptableOptions;
    public List<TileEntityOptionValue> tileEntityOptions;
}

[System.Serializable]
public class TileItemOptionValue<G,T> {
    public G option;
    public T value;
}

[System.Serializable]
public class TileEntityOptionValue {
    public TileEntityOption option;
    public GameObject prefab;
}