using System.Collections.Generic;
using Items;
using Tiles;
using UnityEngine;

namespace World.Cave.Distributors.FluidDistributor
{
    [System.Serializable]
    public class FluidPoolDistribution
    {
        public int Count;
        public int MinSize;
        public int MaxSize;
        public FluidTileItem Item;
    }

    public class FluidPoolDistributor : ICaveDistributor
    {
        private List<FluidPoolDistribution> areaDistributions;

        public FluidPoolDistributor(List<FluidPoolDistribution> areaDistributions)
        {
            this.areaDistributions = areaDistributions;
        }

        public void Distribute(SeralizedWorldData worldData, int width, int height, Vector2Int bottomLeftCorner)
        {
            string[,] fluidIds = worldData.fluidData.ids;
            float[,] fills = worldData.fluidData.fill;
            int pools = 0;
            foreach (FluidPoolDistribution fluidPoolDisruption in areaDistributions)
            {
                string itemId = fluidPoolDisruption.Item?.id;
                if (itemId == null) continue;
                int maxSize = fluidPoolDisruption.MaxSize;
                int minSize = fluidPoolDisruption.MinSize;
                int count = fluidPoolDisruption.Count;
                while (count > 0)
                {
                    int attempts = 256;
                    while (attempts > 0)
                    {
                        int x = Random.Range(0,width); 
                        int y = Random.Range(0,height);
                        HashSet<Vector2Int> fillPositions = CanFill(worldData, width, maxSize, minSize, new Vector2Int(x, y));
                        if (fillPositions != null)
                        {
                            foreach (Vector2Int fillPosition in fillPositions)
                            {
                                fluidIds[fillPosition.x, fillPosition.y] = itemId;
                                fills[fillPosition.x, fillPosition.y] = 1f;
                            }

                            pools++;
                            break;
                        }
                        attempts--;
                    }
                    count--;
                }
            }
            Debug.Log($"Distributed {pools} Fluid Pools in Cave");
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="worldData"></param>
        /// <param name="width"></param>
        /// <param name="maxSize"></param>
        /// <param name="minSize"></param>
        /// <param name="origin"></param>
        /// <returns>A set of positions to fill. If null then is not valid</returns>
        private HashSet<Vector2Int> CanFill(SeralizedWorldData worldData, int width, int maxSize, int minSize, Vector2Int origin)
        {
            int maxY = origin.y;
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();

            bool CanFillPosition(int x, int y)
            {
                if (x < 0 || x >= width || y < 0 || y > maxY) return false;
                if (worldData.fluidData.fill[x, y] > 0) return false;
                string baseId = worldData.baseData.ids[x, y];
                if (baseId == null) return true;
                TileItem tileItem = itemRegistry.GetTileItem(baseId);
                if (tileItem.tileType != TileType.Block) return true;
                if (tileItem.tile is not HammerTile) return false;
                BaseTileData baseTileData = worldData.baseData.sTileOptions[x, y];
                if (baseTileData == null) return true;
                return baseTileData.state != 0;
            }

            if (!CanFillPosition(origin.x, origin.y)) return null;

            HashSet<Vector2Int> seen = new HashSet<Vector2Int>(maxSize);
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            seen.Add(origin);
            queue.Enqueue(origin);
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.left,
                Vector2Int.down,
                Vector2Int.right,
                Vector2Int.up,
            };
            while (queue.Count > 0 && seen.Count < maxSize)
            {
                Vector2Int current = queue.Dequeue();
                foreach (Vector2Int dir in directions)
                {
                    Vector2Int neighbor = current + dir;
                    if (seen.Contains(neighbor)) continue;
                    if (!CanFillPosition(neighbor.x, neighbor.y)) continue;

                    seen.Add(neighbor);
                    queue.Enqueue(neighbor);

                    if (seen.Count >= maxSize) break;
                }
            }

            bool sizeValid = seen.Count >= minSize && seen.Count <= maxSize;
            if (!sizeValid) return null;
            
            Vector2Int[] validateDirections = new Vector2Int[]
            {
                Vector2Int.left,
                Vector2Int.down,
                Vector2Int.right,
            };
            foreach (Vector2Int cellPosition in seen)
            {
                foreach (Vector2Int direction in validateDirections)
                {
                    Vector2Int adj = cellPosition + direction;
                    if (seen.Contains(adj)) continue;
                    if (CanFillPosition(adj.x, adj.y)) return null;
                }
            }
            return seen;

        }
    }
}
