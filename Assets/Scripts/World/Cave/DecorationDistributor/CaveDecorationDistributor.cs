using System;
using System.Collections.Generic;
using Items;
using Tiles;
using UnityEngine;
using Random = System.Random;

namespace World.Cave.DecorationDistributor
{
    public class CaveDecorationDistributor : ICaveDistributor
    {
        private List<CaveDecoration> caveDecorations;

        public CaveDecorationDistributor(List<CaveDecoration> caveDecorations)
        {
            this.caveDecorations = caveDecorations;
        }

        public void Distribute(SeralizedWorldData worldData, int width, int height, Vector2Int bottomLeftCorner)
        {
            if (caveDecorations.Count == 0) return;
            Dictionary<Vector2Int, Direction> positionDirectionDict = new Dictionary<Vector2Int, Direction>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Searching for tiles with an adjacent open space which are not slants/slabs
                    string id = worldData.baseData.ids[x, y];
                    if (id != null) continue;
                    Vector2Int position = new Vector2Int(x, y);
                    Direction? direction = GetDirection(worldData, position,width,height);
                    if (!direction.HasValue) continue;
                    positionDirectionDict[position] = direction.Value;
                }
            }

            foreach (CaveDecoration decoration in caveDecorations)
            {
                float fill = decoration.Fill;
                foreach (var (position, direction) in positionDirectionDict)
                {
                    float ran = UnityEngine.Random.Range(0, 1f);
                    if (ran > fill) continue;
                    worldData.baseData.ids[position.x, position.y] = decoration.TileItem.id;
                    int rotation = GetRotation(direction);
                    worldData.baseData.sTileOptions[position.x, position.y] = new BaseTileData(rotation, 0,false);

                }
            }
        }

        private int GetRotation(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    return 3;
                case Direction.Right:
                    return 1;
                case Direction.Down:
                    return 0;
                case Direction.Up:
                    return 2;
                default:
                    return 0;
            }
        }

        private Direction? GetDirection(SeralizedWorldData worldData, Vector2Int position, int width, int height)
        {
            if (HasAdjacentTile(worldData, position + Vector2Int.up, width, height)) return Direction.Up;
            if (HasAdjacentTile(worldData, position + Vector2Int.down, width, height)) return Direction.Down;
            bool left = HasAdjacentTile(worldData, position + Vector2Int.left, width, height);
            bool right = HasAdjacentTile(worldData, position + Vector2Int.right, width, height);
            if (left && right)
            {
                int ran = UnityEngine.Random.Range(0, 2);
                return ran == 0 ? Direction.Left : Direction.Right;
            }
            if (left) return Direction.Left;
            if (right) return Direction.Right;
            return null;
        }

        private bool HasAdjacentTile(SeralizedWorldData worldData, Vector2Int position, int width, int height)
        {
            if (position.x < 0 || position.y < 0 || position.x >= width || position.y >= height) return false;
            string id = worldData.baseData.ids[position.x, position.y];
            TileItem tileItem = ItemRegistry.GetInstance().GetTileItem(id);
            if (!tileItem) return false;
            BaseTileData baseTileData = worldData.baseData.sTileOptions[position.x, position.y];
            int state = baseTileData?.state ?? 0;
            return state == 0;
        }
    }

    public enum CaveDecorationRestriction
    {
        None,
        Vertical,
        Horizontal,
    }

    [System.Serializable]
    public class CaveDecoration
    {
        public TileItem TileItem;
        [Range(0, 1)] public float Fill;
        public int MaxSize;
        public CaveDecorationRestriction Restriction;
        
    }
}
