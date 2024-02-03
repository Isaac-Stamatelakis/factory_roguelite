using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    protected ChunkSaveOnDestroy chunkSaveOnDestroy;
    public Transform ChunkContainerTransform {get{return chunkContainerTransform;}}
    protected int dim;
    public int Dim {get{return dim;}}

    
    public virtual void Awake () {
        transform.SetParent(GameObject.Find("dim" + dim).transform);
        GameObject chunkContainer = new GameObject();
        chunkContainer.name = "Chunks";
        chunkContainer.transform.SetParent(transform);
        chunkContainerTransform = Global.findChild(gameObject.transform,"Chunks").transform;
        
        chunkLoader = chunkContainerTransform.gameObject.AddComponent<ChunkLoader>();
        chunkUnloader = ChunkContainerTransform.gameObject.AddComponent<ChunkUnloader>();

        partitionLoader = chunkContainerTransform.gameObject.AddComponent<PartitionLoader>();
        partitionUnloader = chunkContainerTransform.gameObject.AddComponent<PartitionUnloader>();
        partitionUnloader.Loader = partitionLoader;
        playerTransform = GameObject.Find("Player").GetComponent<Transform>();

        cachedChunks = new Dictionary<Vector2Int, IChunk>();

        GameObject chunkSave = new GameObject();
        chunkSaveOnDestroy = chunkSave.AddComponent<ChunkSaveOnDestroy>();
        chunkSave.name = "Save";
        chunkSave.transform.SetParent(transform);
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

    public virtual void initalize(IntervalVector coveredArea, int dim)
    {
        Debug.Log("Closed Chunk System Dim:" + dim + " Initalized");
        this.dim = dim;
        this.coveredArea = coveredArea;
        
        StartCoroutine(initalLoad());
        
        /*
        if (false) {
            loadChunksNearPlayer(Mathf.Abs(coveredArea.X.UpperBound-coveredArea.X.LowerBound+1),Mathf.Abs(coveredArea.Y.UpperBound-coveredArea.Y.LowerBound+1));
        } else {
            loadChunksNearPlayer(Global.ChunkLoadRangeX,Global.ChunkLoadRangeY);
        }
        */
        
        
    }

    protected IEnumerator initalLoad() {
        yield return StartCoroutine(initalLoadChunks());
        playerPartitionUpdate();
    }

    public IEnumerator initalLoadChunks() {
        List<Vector2Int> chunks = getUnCachedChunkPositionsNearPlayer();
        foreach (Vector2Int vector in chunks) {
            ChunkIO.getChunkFromJson(vector, this);
        }
        yield return null;
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
            if (!chunk.isChunkLoaded() && !chunk.inRange(playerPosition,Global.ChunkLoadRangeX,Global.ChunkLoadRangeY)) {
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
    }
    public IEnumerator unloadChunkPartition(IChunkPartition chunkPartition) {
        yield return StartCoroutine(chunkPartition.unload(tileGridMaps));
    }

    public void OnDisable()
    {
        foreach (IChunk chunk in cachedChunks.Values) {
            foreach (List<IChunkPartition> chunkPartitionList in chunk.getChunkPartitions()) {
                foreach (IChunkPartition partition in chunkPartitionList) {
                    if (partition.getLoaded()) {
                        partition.save(tileGridMaps);
                    }
                }
            }
            ChunkIO.writeChunk(chunk);
        }
    }


    
}

