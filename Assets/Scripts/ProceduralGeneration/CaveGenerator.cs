using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ChunkModule.IO;
using WaveFunctionCollaps;
using UnityEngine.Tilemaps;

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
        UnityEngine.Random.InitState(seed);
        int[,] noiseField = generateNoiseField();
        int[,] grid = cellular_automaton(noiseField);
        ProcGenHelper.saveToJson(generateWorld(grid),cave,-1);
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
            if (caveArea is CellularCaveArea) {
                CellularCaveArea cellularCaveArea = (CellularCaveArea) caveArea;
                int startX = Global.ChunkSize*(caveArea.xInterval.x-caveMinX);
                int endX =  Global.ChunkSize*(caveArea.xInterval.y-caveMinX);
                int startY = Global.ChunkSize*(caveArea.yInterval.x-caveMinY);
                int endY = Global.ChunkSize*(caveArea.yInterval.y-caveMinY);
                for (int x = startX; x < endX; x ++) {
                    for (int y = startY; y < endY; y++) {
                        float r = UnityEngine.Random.Range(0f, 1f);
                        if (r < cellularCaveArea.fillPercent) {
                            noiseField[x, y] = 1;
                        } else {
                            noiseField[x, y] = 0;
                        }
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
            if (caveArea is CellularCaveArea) {
                CellularCaveArea cellularCaveArea = (CellularCaveArea) caveArea;
                int xStart = 0;
                int xEnd = Global.ChunkSize*(caveArea.xInterval.y-caveArea.xInterval.x+1);
                int yStart = 0;
                int yEnd = Global.ChunkSize*(caveArea.yInterval.y-caveArea.yInterval.x+1);
                int radius = cellularCaveArea.cellRadius;
                int neighboorCount = cellularCaveArea.cellNeighboorCount;
                int xOffset = Global.ChunkSize*(caveArea.xInterval.x-caveMinX);
                int yOffset = Global.ChunkSize*(caveArea.yInterval.x-caveMinY);
                for (int n = 0; n < cellularCaveArea.smoothIterations; n ++) {
                    int[,] tempGrid = new int[xEnd+cellularCaveArea.cellRadius*2, yEnd+cellularCaveArea.cellRadius*2];
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
        } 
        return grid;
    }

    private WorldTileData generateWorld(int[,] grid) {
        // TODO Make it change based on which caveare you are in im lazy for now
        string defaultBlockID = ((CellularCaveArea) cave.areas[0]).defaultBlockID;
        UnityEngine.Vector2Int caveSize = cave.getChunkDimensions();
        int tileMaxX = Global.ChunkSize * caveSize.x;
        int tileMaxY = Global.ChunkSize*caveSize.y;

        SeralizedChunkTileData baseTileData = new SeralizedChunkTileData();
        baseTileData.ids = new List<List<string>>();
        baseTileData.sTileEntityOptions = new List<List<string>>();
        baseTileData.sTileOptions = new List<List<string>>();

        SeralizedChunkTileData backgroundTileData = new SeralizedChunkTileData();
        backgroundTileData.ids = new List<List<string>>();
        backgroundTileData.sTileEntityOptions = new List<List<string>>();
        backgroundTileData.sTileOptions = new List<List<string>>();

        for (int x = 0; x < tileMaxX; x ++) {
            List<string> idListBase = new List<string>();
            List<string> sTileBase = new List<string>();
            List<string> sEntityBase = new List<string>();

            List<string> idListBackground = new List<string>();
            List<string> sEntityBackground = new List<string>();
            List<string> sTileBackground = new List<string>();

            for (int y = 0; y < tileMaxY; y ++) {
                if (grid[x,y] == 1) {
                    idListBase.Add(defaultBlockID);
                } else {
                    idListBase.Add(null);
                }
                sTileBase.Add(null);
                sEntityBase.Add(null);

                idListBackground.Add(null);
                sEntityBackground.Add(null);
                sTileBackground.Add(null);
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


public class WFCGenerator {
    public WFCGenerator(Cave cave,GameObject tileMapPrefab, int patternSize) {
        this.cave = cave;
        this.tileMapPrefab = tileMapPrefab;
        this.patternSize = patternSize;
    }
    private int seed;
    public Cave cave;
    public GameObject tileMapPrefab;
    public Tilemap outputImage;
    public int patternSize;
    public void generate() {
        GameObject temp = new GameObject();
        outputImage = temp.AddComponent<Tilemap>();
        UnityEngine.Random.InitState(seed);
        string[,] strings = generateWFC();
        Debug.Log(strings.Length);
        WorldTileData worldTileData = generateWorld(strings);
        ProcGenHelper.saveToJson(worldTileData,cave,-1);
        GameObject.Destroy(temp);
    }

    public string[,] generateWFC()
    {  
        Vector2Int size = cave.getChunkDimensions() * Global.ChunkSize;
        WaveFunctionCollapse wfc = new WaveFunctionCollapse(
            this.tileMapPrefab.GetComponent<Tilemap>(), 
            this.outputImage, 
            patternSize, 
            size.x, 
            size.y, 
            1, 
            false
        );
        wfc.CreateNewTileMap();
        Tilemap output = wfc.GetOutputTileMap();
        
        string[,] outputStrings = new string[size.x,size.y];
        BoundsInt bounds = output.cellBounds;
        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);

                if (output.HasTile(tilePosition))
                {
                    TileBase tile = output.GetTile(tilePosition);
                    if (tile is StandardTile) {
                        Debug.Log(((StandardTile) tile).id);
                        outputStrings[x-bounds.min.x,y-bounds.min.x]=((StandardTile) tile).id;
                    }
                   
                }
            }
        }
        return outputStrings;
    }

    private WorldTileData generateWorld(string[,] grid) {
        UnityEngine.Vector2Int caveSize = cave.getChunkDimensions();
        int tileMaxX = Global.ChunkSize * caveSize.x;
        int tileMaxY = Global.ChunkSize*caveSize.y;

        SeralizedChunkTileData baseTileData = new SeralizedChunkTileData();
        baseTileData.ids = new List<List<string>>();
        baseTileData.sTileEntityOptions = new List<List<string>>();
        baseTileData.sTileOptions = new List<List<string>>();

        SeralizedChunkTileData backgroundTileData = new SeralizedChunkTileData();
        backgroundTileData.ids = new List<List<string>>();
        backgroundTileData.sTileEntityOptions = new List<List<string>>();
        backgroundTileData.sTileOptions = new List<List<string>>();

        for (int x = 0; x < tileMaxX; x ++) {
            List<string> idListBase = new List<string>();
            List<string> sTileBase = new List<string>();
            List<string> sEntityBase = new List<string>();

            List<string> idListBackground = new List<string>();
            List<string> sEntityBackground = new List<string>();
            List<string> sTileBackground = new List<string>();

            for (int y = 0; y < tileMaxY; y ++) {
                if (grid[x,y] == null || grid[x,y].Length == 0) {
                    idListBase.Add(null);
                } else {
                    idListBase.Add(grid[x,y]);
                }
                
                sTileBase.Add(null);
                sEntityBase.Add(null);

                idListBackground.Add(null);
                sEntityBackground.Add(null);
                sTileBackground.Add(null);
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
}