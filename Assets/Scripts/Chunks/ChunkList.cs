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
    protected List<List<Chunk>> posXposYChunks;
    protected List<List<Chunk>> posXnegYChunks;
    protected List<List<Chunk>> negXposYChunks;
    protected List<List<Chunk>> negXnegYChunks;
    public ChunkList(int maxX, int minX, int maxY, int minY, int dim, ClosedChunkSystem closedChunkSystem) {
        this.closedChunkSystem = closedChunkSystem;
        this.maxX = maxX;
        this.minX = minX;
        this.maxY = maxY;
        this.minY = minY;
        this.dim = dim;
        posXposYChunks = new List<List<Chunk>>();
        posXnegYChunks = new List<List<Chunk>>();
        negXposYChunks = new List<List<Chunk>>();
        negXnegYChunks = new List<List<Chunk>>();

        for (int x = 0 ; x <= maxX; x ++) {
            List<Chunk> tempList = new List<Chunk>();
            for (int y = 0; y <= maxY; y ++) {
                tempList.Add(null);
            }
            posXposYChunks.Add(tempList);
        }
        for (int x = 0 ; x <= maxX; x ++) {
            List<Chunk> tempList = new List<Chunk>();
            for (int y = 0; y <= -minY; y ++) {
                tempList.Add(null);
            }
            posXnegYChunks.Add(tempList);
        }
        for (int x = 0 ; x <= -minX; x ++) {
            List<Chunk> tempList = new List<Chunk>();
            for (int y = 0; y <= maxY; y ++) {
                tempList.Add(null);
            }
            negXposYChunks.Add(tempList);
        }
        for (int x = 0 ; x <= -minX; x ++) {
            List<Chunk> tempList = new List<Chunk>();
            for (int y = 0; y <= -minY; y ++) {
                tempList.Add(null);
            }
            negXnegYChunks.Add(tempList);
        }
    }
    public virtual Chunk GetChunk(int x,int y) {
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
    public virtual void setChunk(Chunk chunk, int x, int y) {
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
                    Chunk[] chunkPropertiesList = chunkGameObject.GetComponents<Chunk>();
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

    public override void setChunk(Chunk chunk, int x, int y)
    {
        base.setChunk(chunk, x, y);
    }
}

public class DungeonChunkList : ChunkList
{
    public Cave cave;
    public DungeonChunkList(Cave cave, int dim, ClosedChunkSystem closedChunkSystem) : base(
        cave.getCoveredArea().X.UpperBound, cave.getCoveredArea().X.LowerBound, cave.getCoveredArea().Y.UpperBound, cave.getCoveredArea().Y.LowerBound, dim, closedChunkSystem
        )
    {
        this.cave = cave;
    }

    public override Chunk GetChunk(int x, int y)
    {
        Chunk chunk = base.GetChunk(x, y);
        if (chunk != null) {
            return chunk;
        }
        return loadChunk(x,y);
    }

    private Chunk loadChunk(int x, int y) {
        if (ChunkIO.jsonExists(new Vector2Int(x,y),dim)) {
            GameObject chunkGameObject = ChunkIO.getChunkFromJson(new Vector2Int(x,y),dim,closedChunkSystem);
            Chunk chunkProperties = chunkGameObject.GetComponent<Chunk>();
            this.setChunk(chunkProperties,x,y);
            return chunkProperties;
        }
        return null;
    }

    public override void initChunks()
    {

        CaveGenerator dungeonGenerator = new CaveGenerator(cave);
        int[,] grid = dungeonGenerator.generate();
        Vector2Int caveSize = cave.getChunkDimensions();
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
                int index = ((chunkX-minX)+(chunkY-minY)*caveSize.x)*Global.ChunkSize;
                for (int tileX = 0; tileX < Global.ChunkSize; tileX ++) {
                    List<string> tempIds = new List<string>();
                    List<Dictionary<string,object>> sEntity = new List<Dictionary<string, object>>();
                    List<Dictionary<string,object>> sTile = new List<Dictionary<string, object>>();
                    for (int  tileY= 0; tileY < Global.ChunkSize; tileY ++) {
                        int tileIndex = index + tileX + tileY * caveSize.x;
                        if (grid[(chunkX-minX)*Global.ChunkSize+tileX,(chunkY-minY)*Global.ChunkSize+tileY] == 1) {
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
                for (int x2 = 0; x2 < Global.ChunkSize; x2 ++) {
                    List<string> tempIds = new List<string>();
                    List<Dictionary<string,object>> sEntity = new List<Dictionary<string, object>>();
                    List<Dictionary<string,object>> sTile = new List<Dictionary<string, object>>();
                    for (int y2 = 0; y2 < Global.ChunkSize; y2 ++) {
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
                for (int x2 = 0; x2 < Global.ChunkSize; x2 ++) {
                    List<string> tempIds = new List<string>();
                    List<Dictionary<string,object>> sEntity = new List<Dictionary<string, object>>();
                    List<Dictionary<string,object>> sTile = new List<Dictionary<string, object>>();
                    for (int y2 = 0; y2 < Global.ChunkSize; y2 ++) {
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
                chunk.initalize(dim,jsonData,new Vector2Int(chunkX,chunkY),this.closedChunkSystem.transform);
                //File.WriteAllText(ChunkIO.getPath(chunk),Newtonsoft.Json.JsonConvert.SerializeObject(jsonData));
                this.setChunk(chunk,chunkX,chunkY);
            }
        }
    }
}
