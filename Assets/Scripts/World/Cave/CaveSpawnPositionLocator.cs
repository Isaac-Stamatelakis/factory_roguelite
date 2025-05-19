using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Items;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace WorldModule.Caves {
     public class CaveSpawnPositionSearcher {
        private SeralizedWorldData worldTileData;
        private Vector2Int bottomLeftCorner;
        private Vector2Int origin;
        private int maxSearchDistance;
        private Vector2Int caveSize;
        private Vector2Int result;
        private List<Vector2Int> directions;
        private int tileSearches;
        public CaveSpawnPositionSearcher(SeralizedWorldData worldTileData, Vector2Int bottomLeftCorner, Vector2Int origin, int maxSearchDistance) {
            this.worldTileData = worldTileData;
            this.bottomLeftCorner = bottomLeftCorner;
            this.origin = origin;
            this.maxSearchDistance = maxSearchDistance;
            int xSize = worldTileData.baseData.ids.GetLength(0);
            int ySize = worldTileData.baseData.ids.GetLength(1);
            this.caveSize = new Vector2Int(xSize,ySize);
            result = origin;
            directions = new List<Vector2Int> {
                Vector2Int.down,
                Vector2Int.up,
                Vector2Int.left,
                Vector2Int.right
            };
            shuffle<Vector2Int>(directions, new System.Random());
            directions.RemoveAt(0);
            
        }
        

        public Vector2Int Search()
        {
            bool found = FindSpawnPosition(origin,false);
            
            bool inFluid = !found; // If first search doesn't work, then check inFluid
            if (inFluid)
            {
                found = FindSpawnPosition(origin, true);
                inFluid = found;
            }
            
            if (found) {
                Debug.Log($"Found cave spawn position {result}. Tiles Searched: {tileSearches}. In-Fluid: {inFluid}.");
                return result;
            }

            Debug.LogWarning("Cave spawn position search could not find alternative position to origin");
            
            const int CLEAR_RANGE = 2;
            for (int x = -CLEAR_RANGE; x <= CLEAR_RANGE; x++)
            {
                for (int y = -1; y < 2*CLEAR_RANGE; y++)
                {
                    Vector2Int clearCellPosition = new Vector2Int(x, y) - bottomLeftCorner;
                    worldTileData.baseData.ids[clearCellPosition.x, clearCellPosition.y] = null;
                }
            }
            
            return Vector2Int.zero;

        }

        private bool FindSpawnPosition(Vector2Int startPosition, bool ignoreFluids)
        {
            var seen = new HashSet<Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(startPosition);

            while (queue.Count > 0)
            {
                Vector2Int position = queue.Dequeue();
                Vector2Int normalizedPosition = position - bottomLeftCorner;

                if (normalizedPosition.x < 0 || normalizedPosition.x >= caveSize.x || 
                    normalizedPosition.y < 0 || normalizedPosition.y >= caveSize.y)
                    continue;

                if (seen.Contains(position))
                    continue;

                seen.Add(position);

                if (seen.Count > maxSearchDistance) continue;
                
                var found = HasSpace(1, normalizedPosition,ignoreFluids);

                if (found)
                {
                    result = position;
                    tileSearches += seen.Count;
                    return true;
                }
                
                
                foreach (Vector2Int direction in directions)
                {
                    queue.Enqueue(position + direction);
                }
            }
            tileSearches += seen.Count;
            return false;
        }

        private void shuffle<T>(List<T> values, System.Random rand)
        {
            int n = values.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (values[i], values[j]) = (values[j], values[i]);
            }
        }

        private bool HasSpace(int radius,Vector2Int position, bool ignoreFluids)
        {
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            for (int x = -radius; x <= radius; x++) {
                for (int y = -radius; y <= radius; y++) {
                    int adjx = position.x+x;
                    int adjy = position.y+y;
                    if (adjx < 0 || adjx >= caveSize.x || adjy < 0 || adjy >= caveSize.y) {
                        return false;
                    }
                    if (worldTileData.baseData.ids[adjx,adjy] != null) {
                        return false;
                    }
                    if (ignoreFluids) continue;
                    string fluidId = worldTileData.fluidData.ids[adjx, adjy];
                    FluidTileItem fluidTileItem = itemRegistry.GetFluidTileItem(fluidId);
                    if (fluidTileItem) return false;
                }
            }

            // Ensure has tiles below 
            int bottomY = position.y-radius-1;
            if (bottomY < 0 || bottomY >= caveSize.y) {
                return false;
            }
            for (int x = -radius; x <= radius; x++) {
                int adjx = position.x+x;
                if (adjx < 0 || adjx >= caveSize.x) {
                    return false;
                }
                
                if (worldTileData.baseData.ids[adjx,bottomY] == null) {
                    return false;
                }
            }
   
            return true;
        }
    }
}