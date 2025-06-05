using System;
using UnityEngine;

namespace World.Structures.Restriction
{
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
}