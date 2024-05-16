using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMaps.Type;
public enum ConduitType {
    Item,
    Fluid,
    Energy,
    Signal,
    Matrix
}
namespace Items {
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
                case ConduitType.Matrix:
                    return TileMapType.MatrixConduit;
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
        public override Sprite[] getSprites()
        {
            return new Sprite[]{ruleTile.m_DefaultSprite};
        }
        public override ItemDisplayType? getDisplayType()
        {
            return ItemDisplayType.Single;
        }

        public TileBase getTile()
        {
            return ruleTile;
        }

        public abstract ConduitType getConduitType();
        public override Sprite getSprite()
        {
            return ruleTile.m_DefaultSprite;
        }
    }
}

