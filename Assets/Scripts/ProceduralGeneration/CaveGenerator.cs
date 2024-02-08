using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ChunkModule.IO;

public class CaveGenerator
{
    public CaveGenerator(Cave cave) {
        this.cave = cave;
    }
    private int seed;
    public Cave cave;

    //float fillPercent = 0.55F;
    
    //int iterations = 7;
    //int iterations = 15; float fillPercent = 0.58F; // Good combo for wide open caves
   // Radius = 2, neighborCount = 14 Good for less connected caves, lots of small rock formations inside larger caves
    //int iterations = 5; float fillPercent = 0.55F; // Not very connected, many small caves
    public void generate() {
        //UnityEngine.Random.InitState(seed);
        int[,] noiseField = generateNoiseField();
        int[,] grid = cellular_automaton(noiseField);
        saveToJson(generateWorld(grid));
    }

    public void saveToJson(WorldTileData worldTileData) {
        UnityEngine.Vector2Int caveSize = cave.getChunkDimensions();
        IntervalVector caveCoveredArea = cave.getCoveredArea();
        int tileMaxX = Global.ChunkSize*caveSize.x;
        int tileMaxY = Global.ChunkSize*caveSize.y;
        int minX = caveCoveredArea.X.LowerBound; int maxX = caveCoveredArea.X.UpperBound;
        int minY = caveCoveredArea.Y.LowerBound; int maxY = caveCoveredArea.Y.UpperBound;
        for (int chunkY = minY; chunkY <= maxY; chunkY ++) {
            for (int chunkX = minX; chunkX <= maxX; chunkX ++) {
                List<ChunkPartitionData> chunkPartitionDataList = new List<ChunkPartitionData>();
                for (int partitionX = 0; partitionX < Global.PartitionsPerChunk; partitionX ++) {
                    for (int partitionY = 0; partitionY < Global.PartitionsPerChunk; partitionY ++) {
                        int xStart = partitionX*Global.ChunkPartitionSize + Global.ChunkSize * (chunkX-minX);
                        int yStart = partitionY*Global.ChunkPartitionSize + Global.ChunkSize * (chunkY-minY);
                        SerializedTileData partitionData = new SerializedTileData();
                        // TODO ENTITIES
                        partitionData.entityData = new List<EntityData>(); 
                        
                        partitionData.baseData = new SeralizedChunkTileData();
                        partitionData.baseData.ids = new List<List<string>>();
                        partitionData.baseData.sTileOptions = new List<List<Dictionary<string, object>>>();
                        partitionData.baseData.sTileEntityOptions = new List<List<string>>();

                        partitionData.backgroundData = new SeralizedChunkTileData();
                        partitionData.backgroundData.ids = new List<List<string>>();
                        partitionData.backgroundData.sTileOptions = new List<List<Dictionary<string, object>>>();
                        partitionData.backgroundData.sTileEntityOptions = new List<List<string>>();

                        for (int tileX = 0; tileX < Global.ChunkPartitionSize; tileX ++) {
                            List<string> idsBase = new List<string>();
                            List<Dictionary<string,object>> sTileOptionsBase = new List<Dictionary<string, object>>();
                            List<string> sTileEntityOptionsBase = new List<string>();

                            List<string> idsBackground = new List<string>();
                            List<Dictionary<string,object>> sTileOptionsBackground = new List<Dictionary<string, object>>();
                            List<string> sTileEntityOptionsBackground = new List<string>();
                            for (int tileY = 0; tileY < Global.ChunkPartitionSize; tileY ++) {
                                int xIndex = xStart+tileX;
                                int yIndex = yStart+tileY;
                                //Debug.Log("Chunk[" + chunkX + "," + chunkY + "], Partition[" + partitionX + "," + partitionY + "], index [" + xIndex + "," + yIndex + "]");
                                idsBase.Add(worldTileData.baseData.ids[xIndex][yIndex]);
                                sTileOptionsBase.Add(worldTileData.baseData.sTileOptions[xIndex][yIndex]);
                                sTileEntityOptionsBase.Add(worldTileData.baseData.sTileEntityOptions[xIndex][yIndex]);

                                idsBackground.Add(worldTileData.backgroundData.ids[xIndex][yIndex]);
                                sTileOptionsBackground.Add(worldTileData.backgroundData.sTileOptions[xIndex][yIndex]);
                                sTileEntityOptionsBackground.Add(worldTileData.backgroundData.sTileEntityOptions[xIndex][yIndex]);

                            }
                            partitionData.baseData.ids.Add(idsBase);
                            partitionData.baseData.sTileOptions.Add(sTileOptionsBase);
                            partitionData.baseData.sTileEntityOptions.Add(sTileEntityOptionsBase);

                            partitionData.backgroundData.ids.Add(idsBackground);
                            partitionData.backgroundData.sTileOptions.Add(sTileOptionsBackground);
                            partitionData.backgroundData.sTileEntityOptions.Add(sTileEntityOptionsBackground);
                        }
                        chunkPartitionDataList.Add(partitionData);
                    }
                }
                ChunkIO.writeNewChunk(new Vector2Int(chunkX,chunkY),-1,chunkPartitionDataList);
            }
        }
    }

    private int[,] generateNoiseField() {
        UnityEngine.Vector2Int caveSize = cave.getChunkDimensions();
        IntervalVector caveCoveredArea = cave.getCoveredArea();
        Debug.Log(caveSize);
        int[,] noiseField = new int[Global.ChunkSize * caveSize.x,Global.ChunkSize*caveSize.y];
        Debug.Log(noiseField.Length);
        int caveMinX = caveCoveredArea.X.LowerBound;
        int caveMinY = caveCoveredArea.Y.LowerBound;
        foreach (CaveArea caveArea in cave.areas) { // fills areas with given density
            int startX = Global.ChunkSize*(caveArea.xInterval.x-caveMinX);
            int endX =  Global.ChunkSize*(caveArea.xInterval.y-caveMinX);
            int startY = Global.ChunkSize*(caveArea.yInterval.x-caveMinY);
            int endY = Global.ChunkSize*(caveArea.yInterval.y-caveMinY);
            for (int x = startX; x < endX; x ++) {
                for (int y = startY; y < endY; y++) {
                    float r = UnityEngine.Random.Range(0f, 1f);
                    if (r < caveArea.fillPercent) {
                        noiseField[x, y] = 1;
                    } else {
                        noiseField[x, y] = 0;
                    }
                }
            }
        }
        return noiseField;
    }
    private int[,] cellular_automaton(int[,] grid) {
        UnityEngine.Vector2Int caveSize = cave.getChunkDimensions();
        int maxX = Global.ChunkSize*caveSize.x;
        int maxY = Global.ChunkSize*caveSize.y;
        IntervalVector caveCoveredArea = cave.getCoveredArea();
        int caveMinX = caveCoveredArea.X.LowerBound;
        int caveMinY = caveCoveredArea.Y.LowerBound;
        
        foreach (CaveArea caveArea in cave.areas) { 
            int xStart = 0;
            int xEnd = Global.ChunkSize*(caveArea.xInterval.y-caveArea.xInterval.x+1);
            int yStart = 0;
            int yEnd = Global.ChunkSize*(caveArea.yInterval.y-caveArea.yInterval.x+1);
            int radius = caveArea.cellRadius;
            int neighboorCount = caveArea.cellNeighboorCount;
            int xOffset = Global.ChunkSize*(caveArea.xInterval.x-caveMinX);
            int yOffset = Global.ChunkSize*(caveArea.yInterval.x-caveMinY);
            for (int n = 0; n < caveArea.smoothIterations; n ++) {
                int[,] tempGrid = new int[xEnd+caveArea.cellRadius*2, yEnd+caveArea.cellRadius*2];
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
        } 
        return grid;
    }

    private WorldTileData generateWorld(int[,] grid) {
        // TODO Make it change based on which caveare you are in im lazy for now
        string defaultBlockID = cave.areas[0].defaultBlockID;
        UnityEngine.Vector2Int caveSize = cave.getChunkDimensions();
        int tileMaxX = Global.ChunkSize * caveSize.x;
        int tileMaxY = Global.ChunkSize*caveSize.y;

        SeralizedChunkTileData baseTileData = new SeralizedChunkTileData();
        baseTileData.ids = new List<List<string>>();
        baseTileData.sTileEntityOptions = new List<List<string>>();
        baseTileData.sTileOptions = new List<List<Dictionary<string, object>>>();

        SeralizedChunkTileData backgroundTileData = new SeralizedChunkTileData();
        backgroundTileData.ids = new List<List<string>>();
        backgroundTileData.sTileEntityOptions = new List<List<string>>();
        backgroundTileData.sTileOptions = new List<List<Dictionary<string, object>>>();

        for (int x = 0; x < tileMaxX; x ++) {
            List<string> idListBase = new List<string>();
            List<Dictionary<string,object>> sTileBase = new List<Dictionary<string, object>>();
            List<string> sEntityBase = new List<string>();

            List<string> idListBackground = new List<string>();
            List<string> sEntityBackground = new List<string>();
            List<Dictionary<string,object>> sTileBackground = new List<Dictionary<string, object>>();

            for (int y = 0; y < tileMaxY; y ++) {
                if (grid[x,y] == 1) {
                    idListBase.Add(defaultBlockID);
                } else {
                    idListBase.Add(null);
                }
                sTileBase.Add(new Dictionary<string, object>());
                sEntityBase.Add(null);

                idListBackground.Add(null);
                sEntityBackground.Add(null);
                sTileBackground.Add(new Dictionary<string, object>());
            }
            baseTileData.ids.Add(idListBase);
            baseTileData.sTileEntityOptions.Add(sEntityBase);
            baseTileData.sTileOptions.Add(sTileBase);

            backgroundTileData.ids.Add(idListBackground);
            backgroundTileData.sTileEntityOptions.Add(sEntityBackground);
            backgroundTileData.sTileOptions.Add(sTileBackground);
        }
        WorldTileData worldTileData = new WorldTileData(
            entityData:new List<EntityData>(),
            baseData: baseTileData, 
            backgroundData: backgroundTileData
        );
        return worldTileData;
    }
    /*
    /// Creates cool line patterns
     private int[,] cool_cellular_automaton(int[,] grid, int count) {
        for (int n = 0; n < count; n ++) {
            for (int x = 0; x < width; x ++) {
                int[,] tempGrid=(int[,])grid.Clone();
                for (int y = 0; y < height; y++) {
                    int neighboors = 0;
                    for (int j = -1; j <= 1; j ++) {
                        for (int k = -1; k <= 1; k ++) {
                            if (j == 0 && k == 0) {
                                continue;
                            }
                            int xIndex = x+j; int yIndex = y+k;
                            if (xIndex < 0  || xIndex >= width || yIndex < 0 || yIndex >= height) {
                                neighboors ++;
                                continue;
                            }
                            if (tempGrid[xIndex,yIndex] == 1) {
                                    neighboors ++;
                            }
                            
                        }
                    }
                    if (neighboors > 4) {
                        grid[x,y] = 0;
                    } else {
                        grid[x,y] = 1;
                    }
                }
            }
        }
        return grid;
    }
     */
}
