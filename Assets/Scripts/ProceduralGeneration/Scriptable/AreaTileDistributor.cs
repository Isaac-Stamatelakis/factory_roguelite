using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ChunkModule.IO;
using System;

namespace WorldModule.Generation {

    public interface IDistributor {
        public void distribute(WorldTileData worldTileData, int seed);
    }
    [CreateAssetMenu(fileName ="New Area Tile Distributor",menuName="Generation/Tile Distributor")]
    public class AreaTileDistributor : ScriptableObject, IDistributor
    {
        public List<TileDistribution> tileDistributions;

        public void distribute(WorldTileData worldTileData, int seed) {
            UnityEngine.Random.InitState(seed);
            SeralizedChunkTileData baseData = worldTileData.baseData;
            int width = baseData.ids.Count;
            int height = baseData.ids[0].Count;
            
            Dictionary<int,List<TileDistribution>> sortedByPriority = getPriorityDict();
            adjustFrequencies(sortedByPriority);
            List<int> prioritiesSorted = sortedByPriority.Keys.ToList();
            prioritiesSorted.Sort();
            string baseID = null;
            foreach (List<string> idList in baseData.ids) {
                if (baseID != null) {
                    break;
                }
                foreach (string id in idList) {
                    if (id != null) {
                        baseID = id;
                        break;
                    }
                }
            }
            List<List<string>> ids = baseData.ids;
            foreach (int priority in prioritiesSorted) {
                List<TileDistribution> distributions = sortedByPriority[priority];
                foreach (TileDistribution distribution in distributions) {
                    for (int x = 0; x < width; x++) {
                        for (int y = 0; y < height; y++) {
                            string id = ids[x][y];
                            if (id == null) { // don't fill into empty space
                                continue;
                            }
                            float ran = UnityEngine.Random.Range(0f,1f);
                            foreach (TileDistribution tileDistribution in distributions) {
                                if (tileDistribution.relativeFrequency < ran) {
                                    continue;
                                }
                                // tile is not base block at position
                                if (!tileDistribution.writeAll && id != baseID) {
                                    continue;
                                }
                                // dimension checks, -1 is ignore
                                if (tileDistribution.minHeight != -1 && y < tileDistribution.minHeight) {
                                    continue;
                                }
                                if (tileDistribution.maxHeight != -1 && y > tileDistribution.maxHeight) {
                                    continue;
                                }
                                if (tileDistribution.minWidth != -1 && x < tileDistribution.minWidth) {
                                    continue;
                                }
                                if (tileDistribution.minHeight != -1 && x > tileDistribution.maxHeight) {
                                    continue;
                                }
                                // Passed all checks
                                int tilesToPlace = UnityEngine.Random.Range(distribution.minimumSize,distribution.maximumSize+1);
                                switch (distribution.searchMode) {
                                    case TileSearchMode.BFS:
                                        BFSTile(distribution, tilesToPlace, x, y, width, height, ids, baseID);
                                        break;
                                    case TileSearchMode.DFS:
                                        DFSTile(distribution, tilesToPlace, x, y, width, height, ids, baseID);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void DFSTile(TileDistribution tileDistribution, int tilesToPlace, int x, int y, int width, int height, List<List<string>> ids, string baseID) {
            List<Direction> directionsToCheck = new List<Direction>(Enum.GetValues(typeof(Direction)).Cast<Direction>());
            while (directionsToCheck.Count > 0) {
                int index = UnityEngine.Random.Range(0,directionsToCheck.Count);
                Direction direction = directionsToCheck[index];
                directionsToCheck.RemoveAt(index);
                string id = null;
                int searchX = x; int searchY = y;
                switch (direction) {
                    case Direction.Up:
                        if (y + 1 < height) {
                            id = ids[x][y+1];
                            searchY++;
                        }
                        break;
                    case Direction.Left:
                        if (x - 1 >= 0) {
                            id = ids[x-1][y];
                            searchX--;
                        }
                        break;
                    case Direction.Down:
                        if (y - 1 >= 0) {
                            id = ids[x][y-1];
                            searchY--;
                        }
                        break;
                    case Direction.Right:
                        if (x + 1 < width) {
                            id = ids[x+1][y];
                            searchX++;
                        }
                        break;
                }
                if (id == null) {
                    continue;
                }
                if (!tileDistribution.writeAll && id != baseID) {
                    continue;
                }
                tilesToPlace--;
                
                if (tilesToPlace < 0) {
                    return;
                }
                string searchId = getSearchID(tileDistribution);
                if (searchId == null) {
                    Debug.LogWarning("Skipped placing tile in tile distributor for " + name);
                    return;
                }
                ids[searchX][searchY] = searchId;
                switch (tileDistribution.searchMode) {
                    case TileSearchMode.BFS:
                        break;
                    case TileSearchMode.DFS:
                        DFSTile(tileDistribution,tilesToPlace,searchX,searchY,width,height,ids,baseID);
                        break;
                }
                
            }
        }

        protected string getSearchID(TileDistribution tileDistribution) {
            float ran = UnityEngine.Random.Range(0f,1f);
            string searchId = null;
            foreach (TileDistributionFrequency tileDistributionFrequency in tileDistribution.tiles) {
                if (ran < tileDistributionFrequency.relativeFrequency) {
                    continue;
                }
                searchId = tileDistributionFrequency.tileItem.id;

            }
            return searchId;
        }
        protected void BFSTile(TileDistribution tileDistribution, int tilesToPlace, int x, int y, int width, int height, List<List<string>> ids, string baseID) {
            Queue<(int x, int y)> queue = new Queue<(int x, int y)>();
            queue.Enqueue((x, y));
            while (queue.Count > 0 && tilesToPlace > 0) {
                (int currentX, int currentY) = queue.Dequeue();
                List<Direction> directionsToCheck = new List<Direction>(Enum.GetValues(typeof(Direction)).Cast<Direction>());

                while (directionsToCheck.Count > 0) {
                    int index = UnityEngine.Random.Range(0, directionsToCheck.Count);
                    Direction direction = directionsToCheck[index];
                    directionsToCheck.RemoveAt(index);

                    int searchX = x, searchY = y;
                    switch (direction) {
                        case Direction.Up:
                            if (currentY + 1 < height) {
                                searchY++;
                            }
                            break;
                        case Direction.Left:
                            if (currentX - 1 >= 0) {
                                searchX--;
                            }
                            break;
                        case Direction.Down:
                            if (currentY - 1 >= 0) {
                                searchY--;
                            }
                            break;
                        case Direction.Right:
                            if (currentX + 1 < width) {
                                searchX++;
                            }
                            break;
                    }

                    if (searchX != x && searchY != y) {
                        string id = ids[searchX][searchY];
                        if (id == baseID || tileDistribution.writeAll) {
                            queue.Enqueue((searchX, searchY));
                            tilesToPlace--;
                            if (tilesToPlace < 0) {
                                return;
                            }
                            string searchId = getSearchID(tileDistribution);
                            if (searchId == null) {
                                Debug.LogWarning("Skipped placing tile in tile distributor for " + name);
                                return;
                            }
                            ids[searchX][searchY] = searchId;
                        }
                    }
                }
            }
        }

        private enum Direction {
            Up,
            Left,
            Down,
            Right
        }
        
        protected Dictionary<int, List<TileDistribution>> getPriorityDict() {
            Dictionary<int, List<TileDistribution>> dict = new Dictionary<int, List<TileDistribution>>();
            foreach (TileDistribution tileDistribution in tileDistributions) {
                if (!dict.ContainsKey(tileDistribution.priority)) {
                    dict[tileDistribution.priority] = new List<TileDistribution>();
                }
                dict[tileDistribution.priority].Add(tileDistribution);
            }

            return dict;
        }

        /// <summary>
        /// Adjusts frequencies 
        /// </summary>
        protected void adjustFrequencies(Dictionary<int,List<TileDistribution>> sortedByPriority) {
            float total = 0f;
            foreach (int priority in sortedByPriority.Keys) {
                float current = 0f;
                int index = 0;
                foreach (TileDistribution distribution in tileDistributions) {
                    float temp = distribution.frequency;
                    distribution.relativeFrequency = current + distribution.frequency;
                    current += distribution.frequency;
                    float tileDistributionFrequencyTotal = 0f;
                    foreach (TileDistributionFrequency tileDistributionFrequency in distribution.tiles) {
                        float temp2 = tileDistributionFrequency.relativeFrequency;
                        tileDistributionFrequency.relativeFrequency += tileDistributionFrequencyTotal;
                        tileDistributionFrequencyTotal += temp2;
                    }
                    if (tileDistributionFrequencyTotal != 1f) {
                        Debug.LogWarning("Tile Distribution frequency " + index + " Doesn't sum to 1f, sums to " + tileDistributionFrequencyTotal);
                    }
                    index++;
                }
                total += current;
            }
            if (total >= 1f) {
                Debug.LogWarning(name + " Distributions cover all base");
            }
            Debug.Log(name + " Distributions Cover " + total*100 + " Percent of Base");
        }
    }

    [System.Serializable]
    public class TileDistribution {
        [Header("Percent of base block this distribution will cover")]
        [Range(0f,1f)]
        public float cover;
        [Header("If false can only write in base block")]
        public bool writeAll;
        [Range(0,3)]
        [Header("Order in which tiles distributions are placed")]
        public int priority;
        [Header("BFS Produces Broad Distributions\nDFS Produces Skinny Long Distributions")]
        public TileSearchMode searchMode;
        [Range(1,1028)]
        public int minimumSize;
        [Range(1,1028)]
        public int maximumSize;
        [Range(-1,1028)]
        public int minHeight = -1;
        [Range(-1,1028)]
        public int maxHeight = -1;
        [Range(-1,1028)]
        public int minWidth = -1;
        [Range(-1,1028)]
        public int maxWidth = -1;
        
        
        public float frequency {get => (minimumSize+maximumSize)/2*cover;}
        public float relativeFrequency;
        public List<TileDistributionFrequency> tiles;
    }

    [System.Serializable]
    public class TileDistributionFrequency {
        public TileItem tileItem;
        [Range(0f,1f)]
        public float relativeFrequency;
    }

    public enum TileSearchMode {
        DFS,
        BFS
    }

}
