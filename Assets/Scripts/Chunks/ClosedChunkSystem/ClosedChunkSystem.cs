using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule;
using ChunkModule.IO;

/// <summary>
/// A closed system of chunks is defined as a system of chunks where every chunk in the system is traversable from every other chunk in the system.
/// A Dimension can have a collection of ClosedChunkSystems 
/// </summary>

public enum MapType {
    Tile,
    Conduit
}
public abstract class ClosedChunkSystem : MonoBehaviour
{
    protected Dictionary<TileMapType, ITileMap> tileGridMaps = new Dictionary<TileMapType, ITileMap>();
    protected Transform playerTransform;
    //public ChunkList chunkList;
    protected Dictionary<Vector2Int, IChunk> cachedChunks;
    protected ChunkLoader chunkLoader;
    protected ChunkUnloader chunkUnloader;
    protected IntervalVector coveredArea;
    protected PartitionLoader partitionLoader;
    protected PartitionUnloader partitionUnloader;    
    protected Transform chunkContainerTransform;
    public Transform ChunkContainerTransform {get{return chunkContainerTransform;}}
    protected int dim;
    public int Dim {get{return dim;}}

    
    public virtual void Awake () {
        
    }

    public void addChunk(IChunk chunk) {
        Vector2Int chunkPosition = chunk.getPosition();
        cachedChunks[chunkPosition] = chunk;
    }
    public void removeChunk(IChunk chunk) {
        Vector2Int chunkPosition = chunk.getPosition();
        if (chunkIsCached(chunkPosition)) {
            cachedChunks.Remove(chunkPosition);
        }
    }

    public IChunk getChunk(Vector2Int position) {
        if (cachedChunks.ContainsKey(position)) {
            return cachedChunks[position];
        }
        return null;
    }

    public bool containsChunk(Vector2Int position) {
        return cachedChunks.ContainsKey(position);
    }

    public bool containsTileMap(TileMapType tileMapType) {
        return tileGridMaps.ContainsKey(tileMapType);
    }

    public ITileMap getTileMap(TileMapType tileMapType) {
        if (tileGridMaps.ContainsKey(tileMapType)) {
            return tileGridMaps[tileMapType];
        }
        return null;
    }
    
    public virtual void initalize(Transform dimTransform, IntervalVector coveredArea, int dim)
    {
        transform.SetParent(dimTransform);
        GameObject chunkContainer = new GameObject();
        chunkContainer.name = "Chunks";
        chunkContainer.transform.SetParent(transform);
        chunkContainerTransform = Global.findChild(gameObject.transform,"Chunks").transform;
        
        chunkLoader = chunkContainerTransform.gameObject.AddComponent<ChunkLoader>();
        chunkLoader.init(this);

        chunkUnloader = ChunkContainerTransform.gameObject.AddComponent<ChunkUnloader>();
        chunkUnloader.init(this);

        partitionLoader = chunkContainerTransform.gameObject.AddComponent<PartitionLoader>();
        partitionLoader.init(this);

        partitionUnloader = chunkContainerTransform.gameObject.AddComponent<PartitionUnloader>();
        partitionUnloader.init(this,partitionLoader);

        playerTransform = GameObject.Find("Player").GetComponent<Transform>();
        cachedChunks = new Dictionary<Vector2Int, IChunk>();
        this.dim = dim;
        this.coveredArea = coveredArea;
        Debug.Log("Closed Chunk System " + name + "For Dim" + dim + " Initalized");
        StartCoroutine(initalLoad());
    }

    protected IEnumerator initalLoad() {
        yield return StartCoroutine(initalLoadChunks());
        playerPartitionUpdate();
        Debug.Log("Partitions Near Player Loaded");
        yield return new WaitForSeconds(1f);
        Debug.Log("Player Activated");
        GameObject.Find("Player").GetComponent<PlayerRobot>().enabled = true;
    }

    protected void initTileMapContainer(TileMapType tileType) {
        GameObject container = new GameObject();
        container.transform.SetParent(gameObject.transform);
        container.name = tileType.ToString();
        container.layer = LayerMask.NameToLayer(tileType.ToString());
        container.transform.localPosition = new Vector3(0,0,TileMapTypeFactory.getTileMapZValue(tileType));
        Grid grid = container.AddComponent<Grid>();
        grid.cellSize = new Vector3(0.5f,0.5f,1f);
        TileGridMap tileGridMap = container.AddComponent<TileGridMap>();
        tileGridMap.type = tileType;
        tileGridMaps[tileType] = tileGridMap;
    }
    public IEnumerator initalLoadChunks() {
        List<Vector2Int> chunks = getUnCachedChunkPositionsNearPlayer();
        foreach (Vector2Int vector in chunks) {
            ChunkIO.getChunkFromJson(vector, this);
        }
        yield return null;
        Debug.Log("Chunks Near Player Loaded");
    }

    
    public virtual void playerChunkUpdate() {
        chunkLoader.addToQueue(getUnCachedChunkPositionsNearPlayer());
        chunkUnloader.addToQueue(getLoadedChunksOutsideRange());
    }

    public virtual void playerPartitionUpdate() {
        List<IChunk> chunksNearPlayer = getLoadedChunksNearPlayer();
        List<IChunkPartition> partitionsToLoad = new List<IChunkPartition>();
        Vector2Int playerPartition = getPlayerChunkPartition();
        foreach (IChunk chunk in chunksNearPlayer) {
            partitionsToLoad.AddRange(chunk.getUnloadedPartitionsCloseTo(playerPartition));
        }
        partitionLoader.addToQueue(partitionsToLoad);

        List<IChunkPartition> partitionsToUnload = new List<IChunkPartition>();
        foreach (IChunk chunk in cachedChunks.Values) {
            partitionsToUnload.AddRange(chunk.getLoadedPartitionsFar(playerPartition));
        }
        partitionUnloader.addToQueue(partitionsToUnload);
    }

    public List<Vector2Int> getUnCachedChunkPositionsNearPlayer() {
        List<Vector2Int> positions = new List<Vector2Int>();
        Vector2Int playerChunkPosition = getPlayerChunk();
        for (int x = playerChunkPosition.x-Global.ChunkLoadRangeX; x  <= playerChunkPosition.x+Global.ChunkLoadRangeX; x ++) {
            for (int y = playerChunkPosition.y-Global.ChunkLoadRangeY; y <= playerChunkPosition.y+Global.ChunkLoadRangeY; y ++) {
                Vector2Int vect = new Vector2Int(x,y);
                if (chunkInBounds(vect) && !chunkIsCached(vect)) {
                    positions.Add(vect);
                }
            }
        }
        return positions;
    }

    public List<IChunk> getLoadedChunksNearPlayer() {
        Vector2Int playerPosition = getPlayerChunk();
        List<IChunk> chunks = new List<IChunk>();
        foreach (IChunk chunk in cachedChunks.Values) {
            if (chunk.inRange(playerPosition,Global.ChunkLoadRangeX,Global.ChunkLoadRangeY)) {
                chunks.Add(chunk);
            }
        }
        return chunks;
    }

    public bool chunkInBounds(Vector2Int position) {
        return 
            position.x >= coveredArea.X.LowerBound && 
            position.x <= coveredArea.X.UpperBound && 
            position.y >= coveredArea.Y.LowerBound && 
            position.y <= coveredArea.Y.UpperBound;
    }
    public bool chunkIsCached(Vector2Int position) {
        return this.cachedChunks.ContainsKey(position);
    }

    
    public List<Chunk> getLoadedChunksOutsideRange() {
        Vector2Int playerPosition = getPlayerChunk();
        List<Chunk> chunksToUnload = new List<Chunk>();
        foreach (Chunk chunk in cachedChunks.Values) {
            if (!chunk.isChunkLoaded() && !chunk.inRange(playerPosition,Global.ChunkLoadRangeX,Global.ChunkLoadRangeY) && chunk.partionsAreAllUnloaded()) {
                chunksToUnload.Add(chunk);
            }
        }
        return chunksToUnload;
    }
    public Vector2Int getPlayerChunk() {
        return new Vector2Int(Mathf.FloorToInt(playerTransform.position.x / ((Global.PartitionsPerChunk >> 1)*Global.ChunkPartitionSize)),Mathf.FloorToInt(playerTransform.position.y / ((Global.PartitionsPerChunk >> 1)*Global.ChunkPartitionSize)));
    }

    public Vector2Int getPlayerChunkPartition() {
        return new Vector2Int(Mathf.FloorToInt(playerTransform.position.x / (Global.ChunkPartitionSize >> 1)),Mathf.FloorToInt(playerTransform.position.y / (Global.ChunkPartitionSize>>1)));
    }

    public IEnumerator loadChunkPartition(IChunkPartition chunkPartition,double angle) {
        yield return chunkPartition.load(tileGridMaps,angle);
        chunkPartition.setTileLoaded(true);
    }
    public IEnumerator unloadChunkPartition(IChunkPartition chunkPartition) {
        yield return StartCoroutine(chunkPartition.unloadTiles(tileGridMaps));
        chunkPartition.setTileLoaded(false);
        chunkPartition.setScheduleForUnloading(false);
    }

    /// <summary> 
    /// This is called when game ends. Saves all partitions
    /// </summary>
    public void OnDisable()
    {
        partitionUnloader.clearAll();
        foreach (IChunk chunk in cachedChunks.Values) {
            foreach (List<IChunkPartition> chunkPartitionList in chunk.getChunkPartitions()) {
                foreach (IChunkPartition partition in chunkPartitionList) {
                    if (partition.getTileLoaded()) {
                        partition.save(tileGridMaps);
                    }
                }
            }
            ChunkIO.writeChunk(chunk);
        }
    }


    
}

