using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule.IO;
using Entities;

namespace WorldModule.Caves {
    public enum RandomType {
        Standard,
        Perlin
    }
    [CreateAssetMenu(fileName ="New Area Tile Distributor",menuName="Generation/Model/Cellular")]
    
    public class CellularGeneratedArea : GenerationModel
    {
        [Header("Base Tile")]
        public TileItem tileItem;
        public int cellRadius;
        public int cellNeighboorCount;
        public float fillPercent;
        public int smoothIterations;
        public RandomType randomType;

        
        public override SerializedTileData generateBase(int seed) {
            UnityEngine.Random.InitState(seed);
            int[,] noiseField = generateNoiseField();
            int[,] grid = cellular_automaton(noiseField);
            return generateWorld(grid);
        }

        

        private int[,] generateNoiseField() {
            UnityEngine.Vector2Int caveSize = getChunkSize();
            IntervalVector caveCoveredArea = getCoveredChunkArea();
            //Debug.Log(caveSize);
            int[,] noiseField = new int[Global.ChunkSize * caveSize.x,Global.ChunkSize*caveSize.y];
            //Debug.Log(noiseField.Length);
            int caveMinX = caveCoveredArea.X.LowerBound;
            int caveMinY = caveCoveredArea.Y.LowerBound;
            int startX = Global.ChunkSize*(xInterval.x-caveMinX);
            int endX =  Global.ChunkSize*(xInterval.y-caveMinX);
            int startY = Global.ChunkSize*(yInterval.x-caveMinY);
            int endY = Global.ChunkSize*(yInterval.y-caveMinY);
            float scale = 0.5f;
            for (int x = startX; x < endX; x ++) {
                for (int y = startY; y < endY; y++) {
                    float r = 0f;
                    switch (randomType) {
                        case RandomType.Standard:
                            r = UnityEngine.Random.Range(0f,1f);
                            break;
                        case RandomType.Perlin:
                            r = Mathf.PerlinNoise(x*scale,y*scale);
                            break;

                    }
                    if (r < fillPercent) {
                        noiseField[x, y] = 0;
                    } else {
                        noiseField[x, y] = 1;
                    }
                }
            }
            
            return noiseField;
        }
        private int[,] cellular_automaton(int[,] grid) {
            UnityEngine.Vector2Int caveSize = getChunkSize();
            int maxX = Global.ChunkSize*caveSize.x;
            int maxY = Global.ChunkSize*caveSize.y;
            IntervalVector caveCoveredArea = getCoveredChunkArea();
            int caveMinX = caveCoveredArea.X.LowerBound;
            int caveMinY = caveCoveredArea.Y.LowerBound;
 
            int xStart = 0;
            int xEnd = Global.ChunkSize*(xInterval.y-xInterval.x+1);
            int yStart = 0;
            int yEnd = Global.ChunkSize*(yInterval.y-yInterval.x+1);
            int radius = cellRadius;
            int neighboorCount = cellNeighboorCount;
            int xOffset = Global.ChunkSize*(xInterval.x-caveMinX);
            int yOffset = Global.ChunkSize*(yInterval.x-caveMinY);
            for (int n = 0; n < smoothIterations; n ++) {
                int[,] tempGrid = new int[xEnd+radius*2, yEnd+radius*2];
                for (int x = xStart; x < xEnd+2*radius; x ++) {
                    for (int y = yStart; y < yEnd+2*radius; y ++) {
                        if (x < radius || x >= xEnd || y < radius || y >= yEnd) {
                            if (x+xOffset < 0 || x+xOffset >= maxX || y+yOffset < 0 || y+yOffset >= maxY) {
                                continue;
                            }
                            tempGrid[x,y] = grid[x+xOffset,y+yOffset];
                        } else {
                            tempGrid[x,y] = grid[x+xOffset,y+yOffset];
                        }
                    }
                }  
                for (int x = xStart; x < xEnd; x ++) {
                    for (int y = yStart; y < yEnd;y++) {
                        int neighboors = 0;
                        for (int j = -radius; j <= radius; j ++) {
                            for (int k = -radius; k <= radius; k ++) {
                                if (j == 0 && k == 0) {
                                    continue;
                                }
                                int xIndex = x+j; 
                                int yIndex = y+k;
                                if (xIndex < 0  || xIndex >= xEnd+radius || yIndex < 0 || yIndex >= yEnd+radius) {
                                    neighboors ++;
                                    continue;
                                }
                                if (tempGrid[xIndex,yIndex] == 1) {
                                    neighboors ++;
                                }
                            }
                        }
                        if (neighboors > neighboorCount) {
                            grid[x+xOffset,y+yOffset] = 0;
                        } else {
                            grid[x+xOffset,y+yOffset] = 1;
                        }
                    }
                }
            } 
            return grid;
        }

        private SerializedTileData generateWorld(int[,] grid) {
            UnityEngine.Vector2Int caveSize = getChunkSize();
            int width = Global.ChunkSize * caveSize.x;
            int height = Global.ChunkSize*caveSize.y;

            SerializedBaseTileData baseTileData = new SerializedBaseTileData();
            baseTileData.ids = new string[width,height];
            baseTileData.sTileEntityOptions = new string[width,height];
            baseTileData.sTileOptions = new string[width,height];

            SerializedBackgroundTileData backgroundTileData = new SerializedBackgroundTileData();
            backgroundTileData.ids = new string[width,height];

            SeralizedFluidTileData fluidTileData = new SeralizedFluidTileData();
            fluidTileData.ids = new string[width,height];
            fluidTileData.fill = new int[width,height];
            for (int x = 0; x < width; x ++) {
                for (int y = 0; y < height; y ++) {
                    if (grid[x,y] == 1) {
                        baseTileData.ids[x,y] = tileItem.id;
                    } 
                }
            }
            SerializedTileData worldTileData = new SerializedTileData(
                baseTileData: baseTileData, 
                backgroundTileData: backgroundTileData,
                entityData: new List<SeralizedEntityData>(),
                fluidTileData: fluidTileData
            );
            return worldTileData;
        }
    }
}

