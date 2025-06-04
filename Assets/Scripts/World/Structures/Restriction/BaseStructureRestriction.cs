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

    public class StructureGroundRestriction : BaseStructureRestriction
    {
        private StructureRestrictionTileType tileType;
        private Direction direction;
        public StructureGroundRestriction(SeralizedWorldData worldData, Vector2Int areaSize, Vector2Int structureSize, StructureRestrictionTileType tileType, Direction direction) : base(worldData, areaSize, structureSize)
        {
            this.tileType = tileType;
            this.direction = direction;
        }

        public override bool ValidateRestriction(Vector2Int spawnPosition)
        {
            if (direction != Direction.Down && direction != Direction.Up)
            {
                throw new ArgumentOutOfRangeException(nameof(direction),"Direction must be up or down");
            }

            int checkY = direction == Direction.Down ? spawnPosition.y - 1 : spawnPosition.y + StructureSize.y + 1;
            if (checkY < 0 || checkY >= AreaSize.y) return false;
            
            for (int x = 0; x < StructureSize.x; x++)
            {
                Vector2Int cellPosition = new Vector2Int(x + spawnPosition.x, checkY);
                if (!HasTile(tileType, cellPosition)) return false;
            }
            
            
            for (int x = 0; x < StructureSize.x; x++)
            {
                for (int y = 0; y < StructureSize.y; y++)
                {
                    Vector2Int cellPosition = new Vector2Int(x, y) + spawnPosition;
                    if (HasTile(tileType, cellPosition)) return false;
                }
            }
            return true;
        }
    }

    public class StructureTileRestriction : BaseStructureRestriction
    {
        StructureRestrictionTileType tileType;
        private bool require;

        public StructureTileRestriction(SeralizedWorldData worldData, Vector2Int areaSize, Vector2Int structureSize, StructureRestrictionTileType tileType, bool require) : base(worldData, areaSize, structureSize)
        {
            this.tileType = tileType;
            this.require = require;
        }

        public override bool ValidateRestriction(Vector2Int spawnPosition)
        {
            for (int x = 0; x < StructureSize.x; x++)
            {
                for (int y = 0; y < StructureSize.y; y++)
                {
                    Vector2Int cellPosition = new Vector2Int(x, y) + spawnPosition;
                    bool has = HasTile(tileType, cellPosition);
                    switch (has)
                    {
                        case false when require:
                        case true when !require:
                            return false;
                    }
                }
            }
            return true;
        }
    }
}
