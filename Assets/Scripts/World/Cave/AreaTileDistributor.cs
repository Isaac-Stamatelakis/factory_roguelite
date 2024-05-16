using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Chunks.IO;
using System;

namespace WorldModule.Caves {

    public interface IDistributor {
        public void distribute(SeralizedWorldData worldTileData, int seed, int width, int height);
    }
    [CreateAssetMenu(fileName ="New Area Tile Distributor",menuName="Generation/Tile Distributor")]
    public class AreaTileDistributor : ScriptableObject, IDistributor
    {
        public List<TileDistribution> tileDistributions;

        public void distribute(SeralizedWorldData worldTileData, int seed, int width, int height) {
            UnityEngine.Random.InitState(seed);
            SerializedBaseTileData baseData = worldTileData.baseData;

            Dictionary<int,List<TileDistribution>> sortedByPriority = getPriorityDict();
            List<int> prioritiesSorted = sortedByPriority.Keys.ToList();
            prioritiesSorted.Sort();
            string baseID = null;
            foreach (string id in baseData.ids) {
                if (baseID != null) {
                    break;
                }
                if (id != null) {
                    baseID = id;
                    break;
                }
            }
            Debug.Log("Base ID loaded as " + baseID);
            string[,] ids = baseData.ids;
            foreach (int priority in prioritiesSorted) {
                List<TileDistribution> distributions = sortedByPriority[priority];
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        string id = ids[x,y];
                        if (id == null) { // don't fill into empty space
                            continue;
                        }
                        float ran = UnityEngine.Random.Range(0f,1f);
                        float cumulativeFrequency = 0f;
                        foreach (TileDistribution tileDistribution in distributions) {
                            cumulativeFrequency += tileDistribution.frequency;
                            if (cumulativeFrequency < ran) {
                                continue;
                            }
                            // tile is not base block at position
                            if (!tileDistribution.writeAll && id != baseID) {
                                continue;
                            }
                            // dimension checks, -1 is ignore
                            if (tileDistribution.useMinHeight && y < tileDistribution.minHeight) {
                                continue;
                            }
                            if (tileDistribution.useMaxHeight && y > tileDistribution.maxHeight) {
                                continue;
                            }
                            if (tileDistribution.useMinWidth && x < tileDistribution.minWidth) {
                                continue;
                            }
                            if (tileDistribution.useMaxWidth && x > tileDistribution.maxWidth) {
                                continue;
                            }
                            // Passed all checks
                            int tilesToPlace = UnityEngine.Random.Range(tileDistribution.minimumSize,tileDistribution.maximumSize+1);
                            switch (tileDistribution.searchMode) {
                                case TileSearchMode.BFS:
                                    BFSTile(tileDistribution, ref tilesToPlace, x, y, width, height, ids, baseID);
                                    break;
                                case TileSearchMode.DFS:
                                    DFSTile(tileDistribution, ref tilesToPlace, x, y, width, height, ids, baseID);
                                    break;
                            }
                        }
                    }
                }
                
            }
        }

        protected void DFSTile(TileDistribution tileDistribution, ref int tilesToPlace, int x, int y, int width, int height, string[,] ids, string baseID) {
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
                            id = ids[x,y+1];
                            searchY++;
                        }
                        break;
                    case Direction.Left:
                        if (x - 1 >= 0) {
                            id = ids[x-1,y];
                            searchX--;
                        }
                        break;
                    case Direction.Down:
                        if (y - 1 >= 0) {
                            id = ids[x,y-1];
                            searchY--;
                        }
                        break;
                    case Direction.Right:
                        if (x + 1 < width) {
                            id = ids[x+1,y];
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
    
                
                string searchId = getSearchID(tileDistribution);
                if (searchId == null) {
                    Debug.LogWarning("Skipped placing tile in tile distributor for " + name);
                    continue;
                }
                if (id == searchId) { // Prevent placing in same id
                    continue;
                }
                tilesToPlace--;
                if (tilesToPlace <= 0) {
                    return;
                }
                ids[searchX,searchY] = searchId;
                
                DFSTile(tileDistribution, ref tilesToPlace,searchX,searchY,width,height,ids,baseID);
            }
        }

        protected string getSearchID(TileDistribution tileDistribution) {
            float ran = UnityEngine.Random.Range(0f,1f);
            string searchId = null;
            float cumulativeFrequency = 0f;

            foreach (TileDistributionFrequency tileDistributionFrequency in tileDistribution.tiles) {
                cumulativeFrequency += tileDistributionFrequency.relativeFrequency;
                if (ran < tileDistributionFrequency.relativeFrequency) {
                    searchId = tileDistributionFrequency.tileItem.id;
                    break;
                }

            }
            return searchId;
        }
        protected void BFSTile(TileDistribution tileDistribution, ref int tilesToPlace, int x, int y, int width, int height, string[,] ids, string baseID) {
            Queue<(int x, int y)> queue = new Queue<(int x, int y)>();
            HashSet<(int x, int y)> visited = new HashSet<(int x, int y)>();
            queue.Enqueue((x, y));
            while (queue.Count > 0 && tilesToPlace > 0) {
                (int currentX, int currentY) = queue.Dequeue();
                List<Direction> directionsToCheck = new List<Direction>(Enum.GetValues(typeof(Direction)).Cast<Direction>());
                
                // Shuffle the directions
                int n = directionsToCheck.Count;
                for (int i = 0; i < n; i++) {
                    int r = i + UnityEngine.Random.Range(0, n - i);
                    Direction temp = directionsToCheck[i];
                    directionsToCheck[i] = directionsToCheck[r];
                    directionsToCheck[r] = temp;
                }
                int directionsToExplore = UnityEngine.Random.Range(0,4);
                foreach (Direction direction in directionsToCheck) {
                    directionsToExplore--;
                    int searchX = currentX, searchY = currentY;
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
                    if (visited.Contains((searchX,searchY))) {
                        continue;
                    }
                    string id = ids[searchX,searchY];
                    if (id == baseID || tileDistribution.writeAll) {
                        queue.Enqueue((searchX, searchY));
                        visited.Add((searchX,searchY));
                        tilesToPlace--;
                        if (tilesToPlace < 0) {
                            break;
                        }
                        string searchId = getSearchID(tileDistribution);
                        if (searchId == null) {
                            Debug.LogWarning("Skipped placing tile in tile distributor for " + name);
                            return;
                        }
                        ids[searchX,searchY] = searchId;
                    }
                    if (directionsToExplore <= 0) {
                        break;
                    }
                }
            }
            // Fills holes
            /*
            HashSet<(int x, int y)> remaining = new HashSet<(int x, int y)>();
            foreach ((int visX,int visY) in visited) {
                List<Direction> directionsToCheck = new List<Direction>(Enum.GetValues(typeof(Direction)).Cast<Direction>());
                foreach (Direction direction in directionsToCheck) {
                    int searchX = visX, searchY = visY;
                    switch (direction) {
                        case Direction.Up:
                            if (visY + 1 < height) {
                                searchY++;
                            }
                            break;
                        case Direction.Left:
                            if (visX - 1 >= 0) {
                                searchX--;
                            }
                            break;
                        case Direction.Down:
                            if (visY - 1 >= 0) {
                                searchY--;
                            }
                            break;
                        case Direction.Right:
                            if (visX + 1 < width) {
                                searchX++;
                            }
                            break;
                    }
                    if (remaining.Contains((searchX,searchY))) {
                        continue;
                    }
                    remaining.Add((searchX,searchY));
                    List<Direction> directionsOfRemaining = new List<Direction>(Enum.GetValues(typeof(Direction)).Cast<Direction>());
                    int count = 0;
                    foreach (Direction direction1 in directionsOfRemaining) {
                        switch (direction1) {
                            case Direction.Up:
                                if (searchY + 1 < height) {
                                    string id = ids[searchX][searchY+1];
                                    if (id == baseID) {
                                        count++;
                                    } 
                                }
                                break;
                            case Direction.Left:
                                if (searchX - 1 >= 0) {
                                    string id = ids[searchX-1][searchY];
                                    if (id == baseID) {
                                        count++;
                                    } 
                                }
                                break;
                            case Direction.Down:
                                if (searchY - 1 >= 0) {
                                    string id = ids[searchX][searchY-1];
                                    if (id == baseID) {
                                        count++;
                                    } 
                                }
                                break;
                            case Direction.Right:
                                if (searchX + 1 < width) {
                                    string id = ids[searchX+1][searchY];
                                    if (id == baseID) {
                                        count++;
                                    } 
                                }
                                break;
                        }
                    }
                    if (count >= 3) {
                        ids[searchX][searchY] = getSearchID(tileDistribution);
                    }
                }
            }
            */

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
        [Range(0,1028)]
        public int minimumSize;
        [Range(0,1028)]
        public int maximumSize;
        [Header("Minimum Height Settings:")]
        public bool useMinHeight;
        [Range(0,32767)]
        public int minHeight = -1;
        [Header("Maximum Height Settings:")]
        public bool useMaxHeight;
        [Range(0,32767)]
        public int maxHeight = -1;
        [Header("Minimum Width Settings:")]
        public bool useMinWidth;
        [Range(0,32767)]
        public int minWidth = -1;
        [Header("Maximum Width Settings:")]
        public bool useMaxWidth;
        [Range(0,32767)]
        public int maxWidth = -1;
        
        
        public float frequency {get =>cover/((minimumSize+maximumSize)/2f);}
        [Header("Tiles to Distribute")]
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
