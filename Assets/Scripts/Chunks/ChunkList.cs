using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
Represents a cartesian plane of chunks.
maxX, minX, maxY, minY are inclusive
Hub Size is x:[-8,8], y:[-4,4]
17 x 9 chunks.
**/
public class ChunkList {
    private int dim;
    private int maxX;
    public int MaxX {
        get {return maxX;}
    }
    private int minX;
    public int MinX {
        get {return minX;}
    }
    private int maxY;
    public int MaxY {
        get {return maxY;}
    }
    private int minY;
    public int MinY {
        get {return minY;}
    }
    private List<List<ChunkProperties>> posXposYChunks;
    private List<List<ChunkProperties>> posXnegYChunks;
    private List<List<ChunkProperties>> negXposYChunks;
    private List<List<ChunkProperties>> negXnegYChunks;
    public ChunkList(int maxX, int minX, int maxY, int minY, int dim) {
        this.maxX = maxX;
        this.minX = minX;
        this.maxY = maxY;
        this.minY = minY;
        this.dim = dim;
        posXposYChunks = new List<List<ChunkProperties>>();
        posXnegYChunks = new List<List<ChunkProperties>>();
        negXposYChunks = new List<List<ChunkProperties>>();
        negXnegYChunks = new List<List<ChunkProperties>>();

        for (int x = 0 ; x <= maxX; x ++) {
            List<ChunkProperties> tempList = new List<ChunkProperties>();
            for (int y = 0; y <= maxY; y ++) {
                tempList.Add(null);
            }
            posXposYChunks.Add(tempList);
        }
        for (int x = 0 ; x <= maxX; x ++) {
            List<ChunkProperties> tempList = new List<ChunkProperties>();
            for (int y = 0; y <= -minY; y ++) {
                tempList.Add(null);
            }
            posXnegYChunks.Add(tempList);
        }
        for (int x = 0 ; x <= -minX; x ++) {
            List<ChunkProperties> tempList = new List<ChunkProperties>();
            for (int y = 0; y <= maxY; y ++) {
                tempList.Add(null);
            }
            negXposYChunks.Add(tempList);
        }
        for (int x = 0 ; x <= -minX; x ++) {
            List<ChunkProperties> tempList = new List<ChunkProperties>();
            for (int y = 0; y <= -minY; y ++) {
                tempList.Add(null);
            }
            negXnegYChunks.Add(tempList);
        }
    }
    public ChunkProperties GetChunk(int x,int y) {
        if (x >= 0 && y >= 0) {
            return posXposYChunks[x][y];
        }
        if (x >= 0 && y < 0) {
            return posXnegYChunks[x][-y];
        }
        if (x < 0 && y >= 0) {
            return negXposYChunks[-x][y];
        } 
        if (x < 0 && y < 0) {
            return negXnegYChunks[-x][-y];
        }
        return null;
    }
    public void setChunk(ChunkProperties chunk, int x, int y) {
        if (x >= 0 && y >= 0) {
            posXposYChunks[x][y] = chunk;
        }
        else if (x >= 0 && y < 0) {
            posXnegYChunks[x][-y] = chunk;
        }
        else if (x < 0 && y >= 0) {
            negXposYChunks[-x][y] = chunk;
        } 
        else if (x < 0 && y < 0) {
            negXnegYChunks[-x][-y] = chunk;
        }
    }

    public bool inChunkBoundary(int chunkX, int chunkY) {
        return ((chunkX >= minX && chunkX <= maxX) && (chunkY >= minY && chunkY <= maxY));
    }
    /**
    Initiates all chunks within boundary inclusive
    **/
    public void initChunks(Transform closedChunkSystem) {
        for (int y = minY; y <= maxY; y ++) {
            for (int x = minX; x <= maxX; x ++) {
                GameObject chunkGameObject = ChunkIO.getChunkFromJson(new Vector2Int(x,y),dim,closedChunkSystem);
                if (chunkGameObject != null) {
                    ChunkProperties[] chunkPropertiesList = chunkGameObject.GetComponents<ChunkProperties>();
                    if (chunkPropertiesList == null || chunkPropertiesList.Length == 0) {
                        Debug.LogError("Failed to import chunk at[" + x + "," + y + "]dim:" + dim);
                        this.setChunk(null,x,y);
                    } else {
                        this.setChunk(chunkPropertiesList[0],x,y);
                    }   
                } else {
                    Debug.LogError("Failed to import chunk at[" + x + "," + y + "]dim:" + dim);
                    this.setChunk(null,x,y);
                }
            }
        }
    }
}
