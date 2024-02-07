using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum IntTileItemOption {
    Hardness = TileItemOption.Hardness,
    Rotation = TileItemOption.Rotation,
    Chisel = TileItemOption.Chisel
}
public enum TileItemOption {
    Rotation,
    Hardness,
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
    Object,
    SlipperyBlock,
    ColladableObject,
    ClimableObject,
    Platform
}

public class TileOptionFactory {

    private static readonly Dictionary<string,TileItemOption> stringOptionDict = new Dictionary<string, TileItemOption>{
        {"Rotation",TileItemOption.Rotation},
    };
    private static readonly HashSet<TileItemOption> dynamic = new HashSet<TileItemOption>{
        TileItemOption.Hardness,
        TileItemOption.Rotation
    };

    private static readonly HashSet<TileItemOption> serizable = new HashSet<TileItemOption>{
        TileItemOption.Rotation
    };

    public static bool isDynamic(TileItemOption option) {
        return dynamic.Contains(option);
    }
    public static bool isSerizable(TileItemOption option) {
        return serizable.Contains(option);
    }
    public static void serializeOption(TileItemOption option, object value, Dictionary<string,object> dict) {
        if (!isSerizable(option)) {
            return;
        }
        dict[option.ToString()] = value;
    } 

    public static Dictionary<string,object> serializeOptions(Dictionary<TileItemOption,object> options) {
        Dictionary<string,object> returnDict = new Dictionary<string, object>();
        foreach (TileItemOption option in options.Keys) {
            if (!isSerizable(option)) {
                continue;
            }
            returnDict[option.ToString()] = options[option];
        }
        return returnDict;
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
[CreateAssetMenu(fileName ="New Tile Item",menuName="Item Register/Tile")]
public class TileItem : ItemObject
{
    public TileType tileType;
    public TileBase tile;
    public TileEntity tileEntity;
    [Tooltip("Specify the integer value for given tile options")]
    public List<TileItemOptionValue<IntTileItemOption,int>> integerOptions = new List<TileItemOptionValue<IntTileItemOption, int>>{
      new TileItemOptionValue<IntTileItemOption, int>(value: 8, option: IntTileItemOption.Hardness) 
    };
    
    public Dictionary<TileItemOption,object> getOptions() {
        Dictionary<TileItemOption, object> dict = new Dictionary<TileItemOption, object>();
        foreach (TileItemOptionValue<IntTileItemOption,int> tileItemOptionValue in integerOptions) {
            TileItemOption option = (TileItemOption)tileItemOptionValue.option;
            dict[option] = tileItemOptionValue.value;
        }
        return dict;
    }

    public override Sprite getSprite()
    {
        if (tile is StandardTile) {
            return ((StandardTile) tile).sprite;
        } else if (tile is AnimatedTile) {
            return ((AnimatedTile) tile).m_AnimatedSprites[0];
        } else if (tile is RuleTile) {
            return ((RuleTile) tile).m_DefaultSprite;
        }
        return null;
    }
}

[System.Serializable]
public class TileItemOptionValue<G,T> {
    public TileItemOptionValue(G option, T value) {
        this.option = option;
        this.value = value;
    }
    public G option;
    public T value;
}
