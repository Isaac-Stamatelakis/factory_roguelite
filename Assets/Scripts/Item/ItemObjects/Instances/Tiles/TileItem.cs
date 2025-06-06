using System;
using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using Item.ItemObjects.Interfaces;
using Item.Slot;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileEntity;
using TileMaps.Type;
using Tiles;
using UnityEngine.AddressableAssets;
using Items;
using Tiles.CustomTiles.IdTiles;
using Tiles.Fluid.Simulation;
using Unity.VisualScripting;

public enum TileMovementType
{
    None,
    Slippery,
    Slow,
    Fast,
    SuperFast,
    LightningFast
}
public enum TileType {
    Block,
    Background,
    Object,
    Platform
}
public static class TileTypeExtension {
    public static TileMapType ToTileMapType(this TileType tileType) {
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
}

[CreateAssetMenu(fileName ="I~New Tile Item",menuName="Item/Instances/Tile")]
public class TileItem : ItemObject, IPlacableItem, ISolidItem, IColorableItem
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

    public override void SetGameStageObject(GameStageObject gameStageObject)
    {
        gameStage = gameStageObject;
    }

    public override Sprite GetSprite()
    {
        return GetDefaultSprite(tile);
    }

    public static Sprite GetDefaultSprite(TileBase tileBase)
    {
        switch (tileBase)
        {
            case Tile tile:
                return tile.sprite;
            case AnimatedTile animatedTile:
                return animatedTile.m_AnimatedSprites[0];
            case RuleTile ruleTile:
                return ruleTile.m_DefaultSprite;
            case IStateTile stateTile:
                return GetDefaultSprite(stateTile.GetDefaultTile());
            case null:
                return null;
            default:
                Debug.LogWarning($"Did not cover case for tilebase {tileBase.name} of type {nameof(tileBase)}");
                return null;
        }
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
    

    public override Sprite[] GetSprites()
    {
        return GetDefaultSprites(tile);
    }

    public TileBase GetTile()
    {
        return tile;
    }

    public Color Color
    {
        get
        {
            if (tileOptions?.TransmutableColorOverride)
            {
                return tileOptions.TransmutableColorOverride.color;
            }

            return tileOptions?.TileColor ? tileOptions.TileColor.GetColor() : UnityEngine.Color.white;
        }
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
