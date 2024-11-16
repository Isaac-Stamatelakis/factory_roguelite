using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMaps.Type;
using Tiles;

public enum ConduitType {
    Item,
    Fluid,
    Energy,
    Signal,
    Matrix
}
namespace Items {
    public static class ConduitTypeExtension {
        public static TileMapType ToTileMapType(this ConduitType conduitType) {
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
        public ConduitStateTile Tile;
        public override Sprite[] getSprites()
        {
            return new Sprite[]{getSprite()};
        }
        public override ItemDisplayType? getDisplayType()
        {
            return ItemDisplayType.Single;
        }

        public TileBase getTile()
        {
            return Tile;
        }

        public abstract ConduitType GetConduitType();
        public override Sprite getSprite()
        {
            if (Tile)
            {
                return Tile.getDefaultSprite();
            }

            return null;
        }
    }
}

