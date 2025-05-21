using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using Item.ItemObjects.Interfaces;
using Item.Slot;
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

    public abstract class ConduitItem : ItemObject, IPlacableItem, ISolidItem
    {
        public GameStageObject GameStageObject;
        public ConduitStateTile Tile;
        public override Sprite[] GetSprites()
        {
            return new Sprite[]{GetSprite()};
        }
        public override ItemDisplayType? getDisplayType()
        {
            return ItemDisplayType.Single;
        }

        public TileBase GetTile()
        {
            return Tile;
        }

        public abstract ConduitType GetConduitType();
        public override Sprite GetSprite()
        {
            return ((Tile)Tile?.GetDefaultTile())?.sprite;
        }

        public override GameStageObject GetGameStageObject()
        {
            return GameStageObject;
        }

        public override void SetGameStageObject(GameStageObject gameStageObject)
        {
            GameStageObject = gameStageObject;
        }
    }
}

