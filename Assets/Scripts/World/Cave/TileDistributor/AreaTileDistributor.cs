using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Chunks.IO;
using Misc.RandomFrequency;
using Misc;
using System;
using Random = UnityEngine.Random;

namespace WorldModule.Caves {
    [CreateAssetMenu(fileName ="New Area Tile Distributor",menuName="Generation/Tile Distributor")]
    public class AreaTileDistributor : CaveTileGenerator
    {
        public List<TileDistribution> tileDistributions;

        /// <summary>
        /// Calculates the number of veins by converting a float to int, then has a chance to add an additional vein equal fNumberOfVeins % 1 
        /// </summary>
        /// <example>5.8 -> 5 + 80% chance to add an additional vein</example>
        /// <param name="fNumberOfVeins"></param>
        private int CalculateNumberOfVeins(float fNumberOfVeins)
        {
            int numberOfVeins = (int)fNumberOfVeins;
            float dif = fNumberOfVeins - numberOfVeins;
            float ran = Random.Range(0, 1f);
            if (ran >= dif) numberOfVeins++; // Randomly increases the veins 
            return numberOfVeins;
        }
        public override void distribute(SeralizedWorldData worldTileData, int width, int height, Vector2Int bottomLeftCorner) {
            SerializedBaseTileData baseData = worldTileData.baseData;
            string baseID = null;
            foreach (string id in baseData.ids)
            {
                if (id == null) continue;
                baseID = id;
                break;
            }
            Debug.Log("Base ID loaded as " + baseID);
            string[,] ids = baseData.ids;
            foreach (TileDistribution tileDistribution in tileDistributions) {
                float chanceToFileTile = tileDistribution.density/(((float)tileDistribution.minimumSize+tileDistribution.maximumSize)/2);
                float fNumberOfVeins = chanceToFileTile * width * height;
                int numberOfVeins = CalculateNumberOfVeins(fNumberOfVeins);
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
            Queue<(int x, int y)> queue = new Queue<(int x, int y)>(); // Isaac 2025: not sure why the fuck I used (int x, int y) but whatever
            HashSet<(int x, int y)> visited = new HashSet<(int x, int y)>();
            
            queue.Enqueue((x, y));
            while (queue.Count > 0 && tilesToPlace > 0) {
                (int currentX, int currentY) = queue.Dequeue();
                List<Direction> directionsToCheck = new List<Direction>(Enum.GetValues(typeof(Direction)).Cast<Direction>());
                
                // Shuffle the directions
                int n = directionsToCheck.Count;
                for (int i = 0; i < n; i++) {
                    int r = i + UnityEngine.Random.Range(0, n - i);
                    (directionsToCheck[i], directionsToCheck[r]) = (directionsToCheck[r], directionsToCheck[i]);
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
                    if (id == null) continue;
                    if (id == baseID || tileDistribution.writeAll) {
                        queue.Enqueue((searchX, searchY));
                        visited.Add((searchX,searchY));
                        tilesToPlace--;
                        if (tilesToPlace < 0) {
                            break;
                        }
                        TileDistributionFrequency tileDistributionElement = RandomFrequencyListUtils.getRandomFromList<TileDistributionFrequency>(tileDistribution.tiles);
                        string searchId = tileDistributionElement.tileItem?.id;
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
            
            HashSet<(int x, int y)> holes = new HashSet<(int x, int y)>();
            
            foreach ((int visX,int visY) in visited)
            {
                TryAddHole(visX+1, visY, holes,visited);
                TryAddHole(visX-1, visY,holes,visited);
                TryAddHole(visX,visY+1,holes,visited);
                TryAddHole(visX,visY-1,holes,visited);
            }
            

            foreach ((int visX, int visY) in holes)
            {
                TileDistributionFrequency tileDistributionElement = RandomFrequencyListUtils.getRandomFromList<TileDistributionFrequency>(tileDistribution.tiles);
                string searchId = tileDistributionElement.tileItem?.id;
                if (searchId == null) {
                    Debug.LogWarning("Skipped placing tile in tile distributor for " + name);
                    return;
                }
                ids[visX,visY] = searchId;
            }
        }

        private bool IsHole(int x, int y, HashSet<(int x, int y)> visited, HashSet<(int x, int y)> holes)
        {
            return !IsFilled(x,y,visited,holes) && IsFilled(x, y + 1, visited, holes) &&
                   IsFilled(x, y - 1, visited, holes) &&
                   IsFilled(x + 1, y, visited, holes) && IsFilled(x-1, y  , visited, holes);
        }

        private bool IsFilled(int x, int y, HashSet<(int x, int y)> visited, HashSet<(int x, int y)> holes)
        {
            return visited.Contains((x,y)) || holes.Contains((x,y));
        }

        private void TryAddHole(int x, int y, HashSet<(int x, int y)> holes, HashSet<(int x, int y)> visited)
        {
            if (!IsHole(x, y, visited,holes)) return;
            holes.Add((x,y));
            
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
