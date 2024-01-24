using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A closed system of chunks is defined as a system of chunks where every chunk in the system is traversable from every other chunk in the system.
/// A Dimension can have a collection of ClosedChunkSystems 
/// ChunkList must be initated
/// </summary>
public abstract class ClosedChunkSystem:  MonoBehaviour
{
    protected Transform playerTransform;
    public ChunkList chunkList;
    protected ChunkLoader chunkLoader;
    protected IntervalVector coveredArea;
    protected ChunkUnloader chunkUnloader;    
    protected int dim;

    
    public virtual void Awake () {
        transform.SetParent(GameObject.Find("dim" + dim).transform);
        GameObject chunkContainer = new GameObject();
        chunkContainer.name = "Chunks";
        chunkContainer.transform.SetParent(transform);
        Transform chunkContainerTransform = Global.findChild(gameObject.transform,"Chunks").transform;
        
        chunkLoader = chunkContainerTransform.gameObject.AddComponent<ChunkLoader>();
        chunkUnloader = chunkContainerTransform.gameObject.AddComponent<ChunkUnloader>();
        chunkUnloader.Loader=chunkLoader;
        playerTransform = GameObject.Find("Player").GetComponent<Transform>();
    }

    public virtual void initalize(IntervalVector coveredArea, int dim, ChunkList chunkList)
    {
        this.chunkList = chunkList;
        if (chunkList == null) {
            Debug.LogError("Attempted to load dim " + dim + " when chunk list was null");
            return;
        }
        chunkList.initChunks();
        this.dim = dim;
        this.coveredArea = coveredArea;
        loadChunksNearPlayer(Global.ChunkLoadRangeX,Global.ChunkLoadRangeY);
        
    }

    public virtual void playerChunkUpdate() {
        chunkLoader.addToQueue(getUnloadedChunksNearPlayer());
        chunkUnloader.addToQueue(getLoadedChunksFar());
    }

    public virtual void OnDisable() {
        for (int x = chunkList.MinX; x <= chunkList.MaxX; x ++) {
            for (int y = chunkList.MinY; y <= chunkList.MaxY; y ++) {
                if (chunkList.GetChunk(x,y).FullLoaded) {
                    ChunkProperties chunkProperties = chunkList.GetChunk(x,y);
                    if (chunkProperties is DynamicChunkProperties) {
                        ((DynamicChunkProperties) chunkProperties).saveToJson();
                    }
                    
                }
            }
        }
    }    
    public List<ChunkProperties> getUnloadedChunksNearPlayer() {
        Vector2Int playerChunk = getPlayerChunk();
        List<ChunkProperties> chunksToLoad = new List<ChunkProperties>();
        for (int x = playerChunk.x-Global.ChunkLoadRangeX; x <= playerChunk.x+Global.ChunkLoadRangeX; x++) {
            for (int y = playerChunk.y-Global.ChunkLoadRangeY; y <= playerChunk.y+Global.ChunkLoadRangeY; y ++) {
                if (chunkList.inChunkBoundary(x,y) && !chunkList.GetChunk(x,y).FullLoaded) {
                    chunksToLoad.Add(chunkList.GetChunk(x,y));
                    chunkList.GetChunk(x,y).ScheduledForUnloading=false;
                }
            }
        }
        return chunksToLoad;
    }
    /**
    Full loads the chunks near the player instantly.
    **/
    protected void loadChunksNearPlayer(int xRange, int yRange) {
        Vector2Int playerChunk = getPlayerChunk();
        for (int x = playerChunk.x-xRange; x <= playerChunk.x+xRange; x++) {
            for (int y = playerChunk.y-yRange; y <= playerChunk.y+yRange; y ++) {
                if (chunkList.inChunkBoundary(x,y)) {
                    ChunkProperties chunkProperties = chunkList.GetChunk(x,y);
                    if (chunkProperties != null) {
                        StartCoroutine(chunkProperties.fullLoadChunk(sectionAmount:16,0));
                    }
                }
            }
        }
    }

    private double distance(Vector2Int playerPosition,ChunkProperties chunk) {
        return Mathf.Pow(playerPosition.x-chunk.ChunkPosition.x,2) + Mathf.Pow(playerPosition.y-chunk.ChunkPosition.y,2);
    }
    /**
    Slowly unloads the chunks far from the player.
    **/
    public List<ChunkProperties> getLoadedChunksFar() {
        int chunkUnloadRange = Global.ChunkLoadRangeX+2;
        Vector2Int playerChunk = getPlayerChunk();
        List<ChunkProperties> chunksToUnload = new List<ChunkProperties>();
        for (int x = chunkList.MinX; x <= chunkList.MaxX; x ++ ){
            for (int y = chunkList.MinY; y <= chunkList.MaxY; y ++) {
                ChunkProperties chunk = chunkList.GetChunk(x,y);
                if (chunk == null) {
                    continue;
                }
                if (chunk.FullLoaded && !chunk.ScheduledForUnloading) {//
                    if (distance(playerChunk,chunk) >= chunkUnloadRange * chunkUnloadRange) {
                        chunksToUnload.Add(chunk);
                    }
                }
            }
        }
        /*
        for (int x = chunkList.MinX; x < playerChunk.x-chunkUnloadRange; x ++) {
            for (int y = chunkList.MinY; y <= chunkList.MaxX; y ++) {
                if (chunkList.inChunkBoundary(x,y) && chunkList.GetChunk(x,y) != null && chunkList.GetChunk(x,y).FullLoaded && !chunkList.GetChunk(x,y).ScheduledForUnloading) {
                    chunksToUnload.Add(chunkList.GetChunk(x,y));
                }
            }
        }
        for (int x = playerChunk.x + chunkUnloadRange+1; x <= chunkList.MaxX; x ++) {
            for (int y = chunkList.MinY; y <= chunkList.MaxX; y ++) {
                if (chunkList.inChunkBoundary(x,y) && chunkList.GetChunk(x,y) != null && chunkList.GetChunk(x,y).FullLoaded && !chunkList.GetChunk(x,y).ScheduledForUnloading) {
                    chunksToUnload.Add(chunkList.GetChunk(x,y));
                }
            }
        }
        for (int x = playerChunk.x - chunkUnloadRange; x < playerChunk.x + chunkUnloadRange; x ++) {
            for (int y = chunkList.MinY; y < playerChunk.y - chunkUnloadRange; y ++) {
                if (chunkList.inChunkBoundary(x,y) && chunkList.GetChunk(x,y) != null && chunkList.GetChunk(x,y).FullLoaded && !chunkList.GetChunk(x,y).ScheduledForUnloading) {
                    chunksToUnload.Add(chunkList.GetChunk(x,y));
                }
            }
        }
        for (int x = playerChunk.x - chunkUnloadRange; x < playerChunk.x + chunkUnloadRange; x ++) {
            for (int y = playerChunk.y + chunkUnloadRange+1; y <= chunkList.MaxY; y ++) {
                if (chunkList.inChunkBoundary(x,y) && chunkList.GetChunk(x,y) != null && chunkList.GetChunk(x,y).FullLoaded && !chunkList.GetChunk(x,y).ScheduledForUnloading) {
                    chunksToUnload.Add(chunkList.GetChunk(x,y));
                }
            }
        }
        */
        return chunksToUnload;
        
    }

    protected void unloadChunksFarFromPlayer() {
        List<ChunkProperties> chunksToUnload = new List<ChunkProperties>();
        Vector2Int playerChunk = getPlayerChunk();
        for (int x = chunkList.MinX; x < playerChunk.x-Global.ChunkLoadRangeX; x ++) {
            for (int y = chunkList.MinY; y <= chunkList.MaxX; y ++) {
                if (chunkList.inChunkBoundary(x,y)) {
                    chunkList.GetChunk(x,y).unfullLoadChunk();
                }
            }
        }
        for (int x = playerChunk.x + Global.ChunkLoadRangeX+1; x <= chunkList.MaxX; x ++) {
            for (int y = chunkList.MinY; y <= chunkList.MaxX; y ++) {
                if (chunkList.inChunkBoundary(x,y)) {
                    chunkList.GetChunk(x,y).unfullLoadChunk();
                }
            }
        }
        for (int x = playerChunk.x - Global.ChunkLoadRangeX; x < playerChunk.x + Global.ChunkLoadRangeX; x ++) {
            for (int y = chunkList.MinY; y < playerChunk.y - Global.ChunkLoadRangeY; y ++) {
                if (chunkList.inChunkBoundary(x,y)) {
                    chunkList.GetChunk(x,y).unfullLoadChunk();
                }
            }
        }
        for (int x = playerChunk.x - Global.ChunkLoadRangeX; x < playerChunk.x + Global.ChunkLoadRangeX; x ++) {
            for (int y = playerChunk.y + Global.ChunkLoadRangeY+1; y <= chunkList.MaxY; y ++) {
                if (chunkList.inChunkBoundary(x,y) && chunkList.GetChunk(x,y)) {
                    chunkList.GetChunk(x,y).unfullLoadChunk();
                }
            }
        }
    }

    public Vector2Int getPlayerChunk() {
        return new Vector2Int(Mathf.FloorToInt(playerTransform.position.x / 8f),Mathf.FloorToInt(playerTransform.position.y / 8f));
    }
}
