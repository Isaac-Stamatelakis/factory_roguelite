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
    [System.Serializable]
    public class TileDistributionData
    {
        [Range(0,1)] public float Fill;
        public bool WriteAll;
        public int MinSize;
        public int MaxSize;
        public TilePlacementMode TilePlacementMode;
        public bool FillHoles = true;
    }

    public interface IDistributorTileAggregator
    {
        public string GetTileId(string currentId);
    }

    public class FrequencyTileAggregator : IDistributorTileAggregator
    {
        public FrequencyTileAggregator(List<TileDistributionFrequency> tiles)
        {
            this.tiles = tiles;
        }

        private List<TileDistributionFrequency> tiles;
        public string GetTileId(string currentId)
        {
            return RandomFrequencyListUtils.getRandomFromList(tiles).tileItem.id;
        }
    }
    [System.Serializable]
    public class TileDistribution
    {
        public TileDistribution(IDistributorTileAggregator distributorTileAggregator, TileDistributionData tileDistributionData)
        {
            TileAggregator = distributorTileAggregator;
            TileDistributionData = tileDistributionData;
        }

        public TileDistributionData TileDistributionData;
        private float chanceToFileTile;
        public IDistributorTileAggregator TileAggregator;
        
        public int CalculateNumberOfVeins(int size)
        {
            chanceToFileTile = TileDistributionData.Fill/(((float)TileDistributionData.MinSize+TileDistributionData.MaxSize)/2);
            float fNumberOfVeins = size * chanceToFileTile;
            int numberOfVeins = (int)fNumberOfVeins;
            float dif = fNumberOfVeins - numberOfVeins;
            float ran = Random.Range(0, 1f);
            if (ran >= dif) numberOfVeins++; // Randomly increases the veins 
            return numberOfVeins;
        }

        public int GetTilesToPlace()
        {
            return UnityEngine.Random.Range(TileDistributionData.MinSize, TileDistributionData.MaxSize+1);
        }
        
    }
    
    public class AreaTileDistributor : ICaveDistributor
    {
        private List<TileDistribution> tileDistributions;
        private string baseID;

        public AreaTileDistributor(List<TileDistribution> tileDistributions, string baseID)
        {
            this.tileDistributions = tileDistributions;
            this.baseID = baseID;
        }

        public void Distribute(SeralizedWorldData worldData, int width, int height, Vector2Int bottomLeftCorner) {
            SerializedBaseTileData baseData = worldData.baseData;
         
            string[,] ids = baseData.ids;
            int size = width * height;
            foreach (TileDistribution tileDistribution in tileDistributions)
            {
                TileDistributionData tileDistributionData = tileDistribution.TileDistributionData;
                int numberOfVeins = tileDistribution.CalculateNumberOfVeins(size);
                while (numberOfVeins > 0) {
                    numberOfVeins--;
                    int x = UnityEngine.Random.Range(0,width);
                    int y = UnityEngine.Random.Range(0,height);
                    string id = ids[x,y];
                    if (id == null) { // don't fill into empty space
                        continue;
                    }
                    if (!tileDistributionData.WriteAll && id != baseID) {
                        continue;
                    } 
                   
                    int tilesToPlace = tileDistribution.GetTilesToPlace();
                    switch (tileDistributionData.TilePlacementMode) {
                        case TilePlacementMode.BreadthFirstSearch:
                            BfsTile(tileDistribution, ref tilesToPlace, x, y, width, height, ids);
                            break;
                        case TilePlacementMode.DepthFirstSearch:
                            DfsTile(tileDistribution, ref tilesToPlace, x, y, width, height, ids);
                            break;
                    }
                }  
            }
        }
        
        protected void DfsTile(TileDistribution tileDistribution, ref int tilesToPlace, int x, int y, int width, int height, string[,] ids) {
            List<Direction> directionsToCheck = new List<Direction>(Enum.GetValues(typeof(Direction)).Cast<Direction>());
            TileDistributionData tileDistributionData = tileDistribution.TileDistributionData;
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
                if (!tileDistributionData.WriteAll && id != baseID) {
                    continue;
                }
                
                ids[searchX, searchY] = tileDistribution.TileAggregator.GetTileId(ids[searchX, searchY]);
                tilesToPlace--;
                if (tilesToPlace <= 0) {
                    return;
                }
                DfsTile(tileDistribution, ref tilesToPlace,searchX,searchY,width,height,ids);
            }
        }
        protected void BfsTile(TileDistribution tileDistribution, ref int tilesToPlace, int x, int y, int width, int height, string[,] ids)
        {
            TileDistributionData tileDistributionData = tileDistribution.TileDistributionData;
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
                    if (id == baseID || tileDistributionData.WriteAll) {
                        queue.Enqueue((searchX, searchY));
                        visited.Add((searchX,searchY));
                        tilesToPlace--;
                        if (tilesToPlace < 0) {
                            break;
                        }
                        ids[searchX, searchY] = tileDistribution.TileAggregator.GetTileId(ids[searchX, searchY]);
                    }
                    if (directionsToExplore <= 0) {
                        break;
                    }
                }
            }

            if (!tileDistributionData.FillHoles) return;
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
                ids[visX, visY] = tileDistribution.TileAggregator.GetTileId(ids[visX, visY]);
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
