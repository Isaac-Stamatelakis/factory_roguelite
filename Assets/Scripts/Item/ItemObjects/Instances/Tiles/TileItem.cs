using System;
using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileEntity;
using TileMaps.Type;
using Tiles;
using UnityEngine.AddressableAssets;
using Items;
using Tiles.CustomTiles.IdTiles;
using Tiles.Fluid.Simulation;

public enum TileMovementType
{
    None,
    Slippery,
    Slow
}
public enum TileType {
    Block,
    Background,
    Object,
    Platform
}
public static class TileTypeExtension {
    public static TileMapType toTileMapType(this TileType tileType) {
        switch (tileType) {
            case TileType.Block:
                return TileMapType.Block;
            case TileType.Background:
                return TileMapType.Background;
            case TileType.Object:
                return TileMapType.Object;
            case TileType.Platform:
                return TileMapType.Platform;
            default:
                Debug.LogError("TileTypeExtension method toTileMapType did not include switch case " + tileType.ToString());
                return TileMapType.Block;
        }
    }

    public static bool isSolid(this TileType tileType) {
        switch (tileType) {
            case TileType.Block:
                return true;
            default:
                return false;
        }
    }
}

[CreateAssetMenu(fileName ="I~New Tile Item",menuName="Item/Instances/Tile")]
public class TileItem : ItemObject, IPlacableItem
{
    public GameStageObject gameStage;
    public TileType tileType;
    public TileBase tile;
    public TileBase outline;
    public TileEntityObject tileEntity;
    public TileOptions tileOptions;
    public override ItemDisplayType? getDisplayType()
    {
        return tile switch
        {
            Tile => ItemDisplayType.Single,
            AnimatedTile animatedTile => ItemDisplayType.Animated,
            RuleTile ruleTile => ItemDisplayType.Single,
            _ => null
        };
    }

    public override GameStageObject GetGameStageObject()
    {
        return gameStage;
    }

    public override Sprite getSprite()
    {
        return GetDefaultSprite(tile);
    }

    public static Sprite GetDefaultSprite(TileBase tileBase)
    {
        return tileBase switch
        {
            Tile tile => tile.sprite,
            AnimatedTile animatedTile => animatedTile.m_AnimatedSprites[0],
            RuleTile ruleTile => ruleTile.m_DefaultSprite,
            IStateTile stateTile => GetDefaultSprite(stateTile.GetDefaultTile()),
            _ => null
        };
    }

    public static Sprite[] GetDefaultSprites(TileBase tileBase)
    {
        return tileBase switch
        {
            Tile standardTile => new Sprite[] { standardTile.sprite },
            AnimatedTile animatedTile => animatedTile.m_AnimatedSprites,
            RuleTile ruleTile => new Sprite[] { ruleTile.m_DefaultSprite },
            IStateTile stateTile => GetDefaultSprites(stateTile.GetDefaultTile()),
            _ => null
        };
    }
    

    public override Sprite[] getSprites()
    {
        return GetDefaultSprites(tile);
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
