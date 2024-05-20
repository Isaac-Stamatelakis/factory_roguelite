using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Chunks.IO;
using Misc.RandomFrequency;
using Misc;
using System;

namespace WorldModule.Caves {
    [CreateAssetMenu(fileName ="New Area Tile Distributor",menuName="Generation/Tile Distributor")]
    public class AreaTileDistributor : CaveTileGenerator
    {
        public List<TileDistribution> tileDistributions;
        public override void distribute(SeralizedWorldData worldTileData, int width, int height, Vector2Int bottomLeftCorner) {
            SerializedBaseTileData baseData = worldTileData.baseData;
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
            foreach (TileDistribution tileDistribution in tileDistributions) {
                double givenDensity = StatUtils.getAmount(tileDistribution.density,tileDistribution.densityStandardDeviation);
                double chanceToFileTile = 1/(givenDensity*(tileDistribution.minimumSize+tileDistribution.maximumSize)/2);
                int numberOfVeins = (int)(chanceToFileTile * width * height);
                while (numberOfVeins > 0) {
                    numberOfVeins--;
                    int x = UnityEngine.Random.Range(0,width);
                    int y = UnityEngine.Random.Range(0,height);
                    string id = ids[x,y];
                    if (id == null) { // don't fill into empty space
                        continue;
                    }
                    if (!tileDistribution.writeAll && id != baseID) {
                        continue;
                    } 
                    checkPlacementRestrictions(tileDistribution,new Vector2Int(x,y) + bottomLeftCorner);
                    int tilesToPlace = UnityEngine.Random.Range(tileDistribution.minimumSize,tileDistribution.maximumSize+1);
                    switch (tileDistribution.placementMode) {
                        case TilePlacementMode.BreadthFirstSearch:
                            BFSTile(tileDistribution, ref tilesToPlace, x, y, width, height, ids, baseID);
                            break;
                        case TilePlacementMode.DepthFirstSearch:
                            DFSTile(tileDistribution, ref tilesToPlace, x, y, width, height, ids, baseID);
                            break;
                    }
                }  
            }
        }

        private bool checkPlacementRestrictions(TileDistribution tileDistribution, Vector2Int position) {
            switch (tileDistribution.restriction) {
                case TilePlacementRestriction.None:
                    return true;
                case TilePlacementRestriction.Vertical:
                    return position.y > tileDistribution.minHeight && position.y < tileDistribution.maxHeight;
                case TilePlacementRestriction.Horizontal:
                    return position.x > tileDistribution.minWidth && position.x < tileDistribution.maxWidth;
                case TilePlacementRestriction.Rectangle:
                    return 
                        position.y > tileDistribution.minHeight && position.y < tileDistribution.maxHeight &&
                        position.x > tileDistribution.minWidth && position.x < tileDistribution.maxWidth;
                case TilePlacementRestriction.Circle:
                    float polarCoordinate = Mathf.Sqrt(position.y * position.y + position.x * position.x); 
                    return polarCoordinate > tileDistribution.minRadius && polarCoordinate < tileDistribution.maxRadius;
            }
            return false;
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
                TileDistributionFrequency tileDistributionElement = RandomFrequencyListUtils.getRandomFromList<TileDistributionFrequency>(tileDistribution.tiles);
                string searchId = tileDistributionElement.tileItem.id;
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
                        TileDistributionFrequency tileDistributionElement = RandomFrequencyListUtils.getRandomFromList<TileDistributionFrequency>(tileDistribution.tiles);
                        string searchId = tileDistributionElement.tileItem.id;
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
    }

    

    public enum TilePlacementMode {
        DepthFirstSearch,
        BreadthFirstSearch
    }

}
