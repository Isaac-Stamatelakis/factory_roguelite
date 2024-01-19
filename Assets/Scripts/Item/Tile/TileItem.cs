using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum IntTileItemOption {
    Rotation = TileItemOption.Rotation,
    Hardness = TileItemOption.Hardness,
    Chisel = TileItemOption.Chisel
}

public enum ScriptableTileItemOption {
    RuleTile = TileItemOption.RuleTile,
    AnimatedTile = TileItemOption.AnimatedTile
}
public enum TileItemOption {
    Rotation,
    Hardness,
    RuleTile,
    AnimatedTile,
    Chisel
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

public class TileEntityOptionFactory {

    private static Dictionary<string,TileItemOption> stringOptionDict = new Dictionary<string, TileItemOption>{
        {"Rotation",TileItemOption.Rotation},
    };
    private static HashSet<TileItemOption> dynamic = new HashSet<TileItemOption>{
        TileItemOption.Hardness,
        TileItemOption.Rotation
    };

    private static HashSet<TileItemOption> serizable = new HashSet<TileItemOption>{
        TileItemOption.Rotation
    };

    public static bool isDynamic(TileItemOption option) {
        return dynamic.Contains(option);
    }
    public static bool isSerizable(TileItemOption option) {
        return dynamic.Contains(option);
    }
    public static void serializeOption(TileItemOption option, object value, Dictionary<string,object> dict) {
        if (!isSerizable(option)) {
            return;
        }
        dict[option.ToString()] = value;
    } 

    public static void deseralizeOptions(Dictionary<string,object> serializedData, Dictionary<TileItemOption, object> dynamicValues) {
        foreach (string key in serializedData.Keys) {
            if (!stringOptionDict.ContainsKey(key)) {
                continue;
            }
            TileItemOption tileItemOption = stringOptionDict[key];
            dynamicValues[tileItemOption] = serializedData[key];
        }
    }
}
public class TileItem : ItemObject
{
    public TileType tileType;
    [Tooltip("Specify the integer value for given tile options")]
    public List<TileItemOptionValue<IntTileItemOption,int>> integerOptions;
    [Tooltip("Specify the integer value for given tile options\nNote if both RuleTile and AnimatedTile are provided, RuleTile is used")]
    public List<TileItemOptionValue<ScriptableTileItemOption, ScriptableObject>> scriptableOptions;
    public List<TileEntityOptionValue> tileEntityOptions;

    public Dictionary<TileItemOption,object> getOptions() {
        Dictionary<TileItemOption, object> dict = new Dictionary<TileItemOption, object>();
        foreach (TileItemOptionValue<IntTileItemOption,int> tileItemOptionValue in integerOptions) {
            TileItemOption option = (TileItemOption)tileItemOptionValue.option;
            dict[option] = tileItemOptionValue.value;
        }
        foreach (TileItemOptionValue<ScriptableTileItemOption,ScriptableObject> tileItemOptionValue in scriptableOptions) {
            TileItemOption option = (TileItemOption)tileItemOptionValue.option;
            dict[option] = tileItemOptionValue.value;
        }
        return dict;
    }
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