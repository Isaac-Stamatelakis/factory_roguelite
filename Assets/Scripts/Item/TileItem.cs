using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileEntityModule;
using TileMapModule.Type;
using Tiles;



public enum TileType {
    Block,
    Background,
    Object,
    SlipperyBlock,
    ColladableObject,
    ClimableObject,
    Platform
}
public static class TileTypeExtension {
    public static TileMapType toTileMapType(this TileType tileType) {
        switch (tileType) {
            case TileType.Block:
                return TileMapType.Block;
            case TileType.Background:
                return TileMapType.Background;
            case TileType.ClimableObject:
                return TileMapType.SlipperyBlock;
            case TileType.ColladableObject:
                return TileMapType.ClimableObject;
            case TileType.Object:
                return TileMapType.Object;
            case TileType.SlipperyBlock:
                return TileMapType.SlipperyBlock;
            case TileType.Platform:
                return TileMapType.Platform;
            default:
                Debug.LogError("TileTypeExtension method toTileMapType did not include switch case " + tileType.ToString());
                return TileMapType.Block;
        }
    }
}

[CreateAssetMenu(fileName ="I~New Tile Item",menuName="Item Register/Tile")]
public class TileItem : ItemObject, IPlacableTile
{
    public TileType tileType;
    public TileBase tile;
    public TileEntity tileEntity;
    [Header("Specify the integer value for given tile options")]
    public List<TileItemOptionValue<IntItemOption,int>> integerOptions = new List<TileItemOptionValue<IntItemOption, int>>{
      new TileItemOptionValue<IntItemOption, int>(value: 8, option: IntItemOption.Hardness) 
    };
    [Header("Specify if have bool options")]
    public List<BoolItemOption> boolOptions = new List<BoolItemOption>();
    [Header("Give drop values on break\nLeave empty to drop itself\n")]
    public List<TileItemOptionValue<string, int>> dropOptions;
    
    public TileOptions initOptions() {
        Dictionary<TileOption, object> options = getNonSOptions();
        Dictionary<TileOption, object> sOptions = new Dictionary<TileOption, object>();
       
        foreach (BoolItemOption boolItemOption in boolOptions) {
            sOptions[(TileOption) boolItemOption] = 0;
        }
        return new TileOptions(options,sOptions);
    }
    public Dictionary<TileOption, object> getNonSOptions() {
        Dictionary<TileOption, object> options = new Dictionary<TileOption, object>();
        foreach (TileItemOptionValue<IntItemOption,int> tileItemOptionValue in integerOptions) {
            TileOption option = (TileOption)tileItemOptionValue.option;
            options[option] = tileItemOptionValue.value;
        }
        if (dropOptions.Count != 0) {
            options[TileOption.Drop] = dropOptions;
        }
        return options;
    }

    public bool containsBoolOption(BoolItemOption tileItemOption) {
        return boolOptions.Contains(tileItemOption);
    }

    public override Sprite getSprite()
    {
        if (tile is StandardTile standardTile) {
            return standardTile.sprite;
        } else if (tile is AnimatedTile animatedTile) {
            return animatedTile.m_AnimatedSprites[0];
        } else if (tile is RuleTile ruleTile) {
            return ruleTile.m_DefaultSprite;
        } else if (tile is RandomTile randomTile) {
            return randomTile.sprite;
        } else if (tile is IRestrictedTile restrictedTile) {
            return restrictedTile.getSprite();
        }
        return null;
    }

    public TileBase getTile()
    {
        return tile;
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
