using UnityEngine;

namespace World.Structures.Restriction
{
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