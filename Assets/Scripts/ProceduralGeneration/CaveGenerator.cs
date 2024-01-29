using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    public int[,] generate() {
        //UnityEngine.Random.InitState(seed);
        int[,] noiseField = generateNoiseField();
        int[,] grid = cellular_automaton(noiseField);
        return noiseField;
    }

    private int[,] generateNoiseField() {
        Vector2Int caveSize = cave.getChunkDimensions();
        IntervalVector caveCoveredArea = cave.getCoveredArea();
        Debug.Log(caveSize);
        int[,] noiseField = new int[Global.ChunkSize* caveSize.x,Global.ChunkSize*caveSize.y];
        Debug.Log(noiseField.Length);
        int caveMinX = caveCoveredArea.X.LowerBound;
        //Debug.Log(caveCoveredArea.X.LowerBound + "," + caveCoveredArea.X.UpperBound + ";" + caveCoveredArea.Y.LowerBound + "," + caveCoveredArea.Y.UpperBound);
        int caveMinY = caveCoveredArea.Y.LowerBound;
        Debug.Log(caveMinX + "," +caveMinY);
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
        Vector2Int caveSize = cave.getChunkDimensions();
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
            int xOffset = 16*(caveArea.xInterval.x-caveMinX);
            int yOffset = 16*(caveArea.yInterval.x-caveMinY);
            Debug.Log(xOffset + "," + yOffset);
            Debug.Log(grid.Length);
            for (int n = 0; n < caveArea.smoothIterations; n ++) {
                int[,] tempGrid = new int[xEnd+caveArea.cellRadius*2, yEnd+caveArea.cellRadius*2];
                Debug.Log(tempGrid.Length);
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
