using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace WorldModule.Caves {
    
    public static class CaveUtils {
        public static IEnumerable LoadCaveElements(Cave cave) {
            var entityHandle = cave.entityDistributor.LoadAssetAsync<UnityEngine.Object>();
            var generationModelHandle = cave.generationModel.LoadAssetAsync<UnityEngine.Object>();
            
            yield return entityHandle;
            yield return generationModelHandle;
            
        }
    
        
    }

    public class CaveSpawnPositionSearcher {
        private SeralizedWorldData worldTileData;
        private Vector2Int bottomLeftCorner;
        private Vector2Int origin;
        private int maxSearchDistance;
        private HashSet<Vector2Int> seen = new HashSet<Vector2Int>();
        private Vector2Int caveSize;
        private Vector2Int result;
        private List<Vector2Int> directions;
        private bool found;
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
            
        }

        public Vector2Int search() {
            FindSpawnPosition(origin);
            if (found) {
                Debug.Log($"Found cave spawn position {result} after {seen.Count} searchs");
            } else {
                Debug.LogWarning("Cave spawn position search could not find alternative position to origin");
            }
            return result;
        }

        public void FindSpawnPosition(Vector2Int startPosition)
        {
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(startPosition);

            while (queue.Count > 0)
            {
                Vector2Int position = queue.Dequeue();

                if (found) 
                {
                    return;
                }

                Vector2Int normalizedPosition = position - bottomLeftCorner;

                if (normalizedPosition.x < 0 || normalizedPosition.x >= caveSize.x || 
                    normalizedPosition.y < 0 || normalizedPosition.y >= caveSize.y)
                    continue;

                if (seen.Contains(position))
                    continue;

                seen.Add(position);

                if (seen.Count > maxSearchDistance)
                    continue;

                found = hasSpace(1, normalizedPosition);
                result = position;

                foreach (Vector2Int direction in directions)
                {
                    queue.Enqueue(position + direction);
                }
            }
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

        private bool hasSpace(int radius,Vector2Int position) {
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

