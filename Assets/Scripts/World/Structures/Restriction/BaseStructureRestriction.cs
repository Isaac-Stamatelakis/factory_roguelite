using System;
using Items;
using Unity.VisualScripting;
using UnityEngine;

namespace World.Structures.Restriction
{
    public enum StructureRestrictionTileType
    {
        Block,
        Fluid,
        All
    }
    public abstract class BaseStructureRestriction
    {
        protected readonly SeralizedWorldData WorldData;
        protected Vector2Int AreaSize;
        protected Vector2Int StructureSize;
        protected ItemRegistry ItemRegistry;
        
        protected BaseStructureRestriction(SeralizedWorldData worldData, Vector2Int areaSize, Vector2Int structureSize)
        {
            WorldData = worldData;
            AreaSize = areaSize;
            StructureSize = structureSize;
            ItemRegistry = ItemRegistry.GetInstance();
        }

        public abstract bool ValidateRestriction(Vector2Int spawnPosition);
        protected bool HasTile(StructureRestrictionTileType tileType, Vector2Int position)
        {
            switch (tileType)
            {
                case StructureRestrictionTileType.Block:
                    string id = WorldData.baseData.ids[position.x, position.y];
                    TileItem tileItem = ItemRegistry.GetTileItem(id);
                    if (!tileItem) return false;
                    return tileItem.tileType == TileType.Block;
                case StructureRestrictionTileType.Fluid:
                    return WorldData.fluidData.ids[position.x,position.y] != null;
                case StructureRestrictionTileType.All:
                    return HasTile(StructureRestrictionTileType.Block, position) ||
                           HasTile(StructureRestrictionTileType.Fluid, position);
                default:
                    throw new ArgumentOutOfRangeException(nameof(tileType), tileType, null);
            }
        }
    }
}
