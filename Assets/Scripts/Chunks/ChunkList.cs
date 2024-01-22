using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
/**
Represents a cartesian plane of chunks.
maxX, minX, maxY, minY are inclusive
Hub Size is x:[-8,8], y:[-4,4]
17 x 9 chunks.
**/
public abstract class ChunkList {
    protected ClosedChunkSystem closedChunkSystem;
    protected int dim;
    protected int maxX;
    public int MaxX {
        get {return maxX;}
    }
    protected int minX;
    public int MinX {
        get {return minX;}
    }
    protected int maxY;
    public int MaxY {
        get {return maxY;}
    }
    protected int minY;
    public int MinY {
        get {return minY;}
    }
    protected List<List<ChunkProperties>> posXposYChunks;
    protected List<List<ChunkProperties>> posXnegYChunks;
    protected List<List<ChunkProperties>> negXposYChunks;
    protected List<List<ChunkProperties>> negXnegYChunks;
    public ChunkList(int maxX, int minX, int maxY, int minY, int dim, ClosedChunkSystem closedChunkSystem) {
        this.closedChunkSystem = closedChunkSystem;
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
    public virtual ChunkProperties GetChunk(int x,int y) {
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
    public virtual void setChunk(ChunkProperties chunk, int x, int y) {
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
    
    public abstract void initChunks();
}

public class PreGeneratedChunkList : ChunkList
{
    public PreGeneratedChunkList(int maxX, int minX, int maxY, int minY, int dim, ClosedChunkSystem closedChunkSystem) : base(maxX, minX, maxY, minY, dim, closedChunkSystem)
    {
        
    }
    /// </summary>
    /// Loads all chunks from given dim folder
    /// <summary>
    public override void initChunks()
    {
        string filePath = Application.dataPath + "/Resources/worlds/" + Global.WorldName + "/Chunks/dim" + dim;
        if (Directory.Exists(filePath))
        {
            Debug.Log("Reading dimension data from path: " + filePath);
        } else {
            Debug.Log("Created new dimension directory: " + filePath);
            Directory.CreateDirectory(filePath);
        }
        for (int y = minY; y <= maxY; y ++) {
            for (int x = minX; x <= maxX; x ++) {
                GameObject chunkGameObject = ChunkIO.getChunkFromJson(new Vector2Int(x,y),dim,this.closedChunkSystem);
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

    public override void setChunk(ChunkProperties chunk, int x, int y)
    {
        base.setChunk(chunk, x, y);
    }
}

public class DungeonChunkList : ChunkList
{

    public DungeonChunkList(int maxX, int minX, int maxY, int minY, int dim, ClosedChunkSystem closedChunkSystem) : base(maxX, minX, maxY, minY, dim, closedChunkSystem)
    {
    }

    public override ChunkProperties GetChunk(int x, int y)
    {
        ChunkProperties chunk = base.GetChunk(x, y);
        if (chunk != null) {
            return chunk;
        }
        return loadChunk(x,y);
    }

    private ChunkProperties loadChunk(int x, int y) {
        if (ChunkIO.jsonExists(new Vector2Int(x,y),dim)) {
            GameObject chunkGameObject = ChunkIO.getChunkFromJson(new Vector2Int(x,y),dim,closedChunkSystem);
            ChunkProperties chunkProperties = chunkGameObject.GetComponent<ChunkProperties>();
            this.setChunk(chunkProperties,x,y);
            return chunkProperties;
        }
        return null;
    }

    public override void initChunks()
    {
        int width = (Mathf.Abs(maxX-minX)+1)*16;
        int height = (Mathf.Abs(maxY-minY)+1)*16;
        DungeonGenerator dungeonGenerator = new DungeonGenerator(width,height);
        int[,] grid = dungeonGenerator.generate();
        for (int chunkY = minY; chunkY <= maxY; chunkY ++) {
            for (int chunkX = minX; chunkX <= maxX; chunkX ++) {
                GameObject chunkObject = new GameObject();
                DynamicChunkProperties chunk = chunkObject.AddComponent<DynamicChunkProperties>();
                chunk.name = "chunk[" + chunkX + "," + chunkY + "]";
                JsonData jsonData = new JsonData();
                SeralizedChunkTileData tileData = new SeralizedChunkTileData();
                tileData.ids = new List<List<string>>();
                tileData.sTileEntityOptions = new List<List<Dictionary<string, object>>>();
                tileData.sTileOptions = new List<List<Dictionary<string, object>>>();
                int index = ((chunkX-minX)+(chunkY-minY)*width)*16;
                for (int tileX = 0; tileX < 16; tileX ++) {
                    List<string> tempIds = new List<string>();
                    List<Dictionary<string,object>> sEntity = new List<Dictionary<string, object>>();
                    List<Dictionary<string,object>> sTile = new List<Dictionary<string, object>>();
                    for (int  tileY= 0; tileY < 16; tileY ++) {
                        int tileIndex = index + tileX + tileY * width;
                        if (grid[(chunkX-minX)*16+tileX,(chunkY-minY)*16+tileY] == 1) {
                            tempIds.Add("weird_stone1");
                        } else {
                            tempIds.Add(null);
                        }
                        sTile.Add(new Dictionary<string, object>());
                        sEntity.Add(new Dictionary<string, object>());
                    }
                    tileData.ids.Add(tempIds);
                    tileData.sTileEntityOptions.Add(sEntity);
                    tileData.sTileOptions.Add(sTile);
                }
                SeralizedChunkTileData backgroundData = new SeralizedChunkTileData();
                backgroundData.ids = new List<List<string>>();
                backgroundData.sTileEntityOptions = new List<List<Dictionary<string, object>>>();
                backgroundData.sTileOptions = new List<List<Dictionary<string, object>>>();
                for (int x2 = 0; x2 < 16; x2 ++) {
                    List<string> tempIds = new List<string>();
                    List<Dictionary<string,object>> sEntity = new List<Dictionary<string, object>>();
                    List<Dictionary<string,object>> sTile = new List<Dictionary<string, object>>();
                    for (int y2 = 0; y2 < 16; y2 ++) {
                        tempIds.Add(null);
                        sTile.Add(new Dictionary<string, object>());
                        sEntity.Add(new Dictionary<string, object>());
                    }
                    backgroundData.ids.Add(tempIds);
                    backgroundData.sTileEntityOptions.Add(sEntity);
                    backgroundData.sTileOptions.Add(sTile);
                }
                SeralizedChunkTileData objectData = new SeralizedChunkTileData();
                objectData.ids = new List<List<string>>();
                objectData.sTileEntityOptions = new List<List<Dictionary<string, object>>>();
                objectData.sTileOptions = new List<List<Dictionary<string, object>>>();
                for (int x2 = 0; x2 < 16; x2 ++) {
                    List<string> tempIds = new List<string>();
                    List<Dictionary<string,object>> sEntity = new List<Dictionary<string, object>>();
                    List<Dictionary<string,object>> sTile = new List<Dictionary<string, object>>();
                    for (int y2 = 0; y2 < 16; y2 ++) {
                        tempIds.Add(null);
                        sTile.Add(new Dictionary<string, object>());
                        sEntity.Add(new Dictionary<string, object>());
                    }
                    objectData.ids.Add(tempIds);
                    objectData.sTileEntityOptions.Add(sEntity);
                    objectData.sTileOptions.Add(sTile);
                }
                jsonData.dict["TileBlocks"] = tileData;
                jsonData.dict["TileBackgrounds"] = backgroundData;
                jsonData.dict["TileObjects"] = objectData;
                jsonData.dict["Entities"] = new List<EntityData>();
                chunk.initalize(dim,new Vector2Int(chunkX,chunkY),jsonData,this.closedChunkSystem.transform);
                this.setChunk(chunk,chunkX,chunkY);
            }
        }
    }
}
