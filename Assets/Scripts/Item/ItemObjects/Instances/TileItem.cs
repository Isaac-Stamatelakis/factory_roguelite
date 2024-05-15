using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileEntityModule;
using TileMaps.Type;
using Tiles;
using Items;

public enum TileType {
    Block,
    Background,
    Object,
    SlipperyBlock,
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

    public static bool isSolid(this TileType tileType) {
        switch (tileType) {
            case TileType.Block:
                return true;
            case TileType.SlipperyBlock:
                return true;
            default:
                return false;
        }
    }
}

[CreateAssetMenu(fileName ="I~New Tile Item",menuName="Item/Instances/Tile")]
public class TileItem : ItemObject, IPlacableItem
{
    public TileType tileType;
    public TileBase tile;
    public TileEntity tileEntity;
    public TileOptions tileOptions;
    public override ItemDisplayType? getDisplayType()
    {
        if (tile is StandardTile standardTile) {
            return ItemDisplayType.Single;
        } else if (tile is AnimatedTile animatedTile) {
            return ItemDisplayType.Animated;
        } else if (tile is RuleTile ruleTile) {
            return ItemDisplayType.Single;
        } else if (tile is RandomTile randomTile) {
            return ItemDisplayType.Single;
        } else if (tile is IStateTile stateTile) {
            return ItemDisplayType.Single;
        }
        return null;
    }

    public override Sprite getSprite() {
        if (tile is StandardTile standardTile) {
            return standardTile.sprite;
        } else if (tile is AnimatedTile animatedTile) {
            return animatedTile.m_AnimatedSprites[0];
        } else if (tile is RuleTile ruleTile) {
            return ruleTile.m_DefaultSprite;
        } else if (tile is RandomTile randomTile) {
            return randomTile.sprite;
        } else if (tile is IStateTile stateTile) {
            return stateTile.getDefaultSprite();
        }
        return null;
    }

    public override Sprite[] getSprites()
    {
        if (tile is StandardTile standardTile) {
            return new Sprite[]{standardTile.sprite};
        } else if (tile is AnimatedTile animatedTile) {
            return animatedTile.m_AnimatedSprites;
        } else if (tile is RuleTile ruleTile) {
            return new Sprite[]{ruleTile.m_DefaultSprite};
        } else if (tile is RandomTile randomTile) {
            return new Sprite[]{randomTile.sprite};
        } else if (tile is IStateTile stateTile) {
            return new Sprite[]{stateTile.getDefaultSprite()};
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
