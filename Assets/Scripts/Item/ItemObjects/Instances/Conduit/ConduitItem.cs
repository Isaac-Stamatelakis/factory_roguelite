using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMapModule.Type;

public enum ConduitType {
    Item,
    Fluid,
    Energy,
    Signal,
    Matrix
}

public static class ConduitTypeExtension {
    public static TileMapType toTileMapType(this ConduitType conduitType) {
        switch (conduitType) {
            case ConduitType.Item:
                return TileMapType.ItemConduit;
            case ConduitType.Fluid:
                return TileMapType.FluidConduit;
            case ConduitType.Energy:
                return TileMapType.EnergyConduit;
            case ConduitType.Signal:
                return TileMapType.SignalConduit;
            default:
                Debug.LogError("ConduitTypeExtension method toTileMapType did not include switch case " + conduitType.ToString());
                return TileMapType.ItemConduit;
        }
    }
}

public enum ResourceConduitType {
    Item = ConduitType.Item,
    Fluid = ConduitType.Fluid,
    Energy = ConduitType.Energy
}

public abstract class ConduitItem : ItemObject, IPlacableItem
{
    public RuleTile ruleTile;
    public int hardness;
    public override Sprite getSprite()
    {
        return ruleTile.m_DefaultSprite;
    }

    public TileBase getTile()
    {
        return ruleTile;
    }

    public abstract ConduitType getType();
}


