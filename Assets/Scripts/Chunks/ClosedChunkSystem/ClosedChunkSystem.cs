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
        if (false) {
            loadChunksNearPlayer(20,20);
        } else {
            loadChunksNearPlayer(Global.ChunkLoadRange,Global.ChunkLoadRange);
        }
        
        
    }

    public virtual void playerChunkUpdate() {
        StartCoroutine(chunkLoader.load(getUnloadedChunksNearPlayer()));
        StartCoroutine(chunkUnloader.unload(getLoadedChunksFar()));
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
        for (int x = playerChunk.x-Global.ChunkLoadRange; x <= playerChunk.x+Global.ChunkLoadRange; x++) {
            for (int y = playerChunk.y-Global.ChunkLoadRange; y <= playerChunk.y+Global.ChunkLoadRange; y ++) {
                if (chunkList.inChunkBoundary(x,y) && !chunkList.GetChunk(x,y).FullLoaded) {
                    chunksToLoad.Add(chunkList.GetChunk(x,y));
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
                        chunkProperties.fullLoadChunk(sectionAmount:16,Vector2Int.zero);
                    }
                }
            }
        }
    }

    /**
    Slowly unloads the chunks far from the player.
    **/
    public List<ChunkProperties> getLoadedChunksFar() {
        int chunkUnloadRange = Global.ChunkLoadRange+2;
        Vector2Int playerChunk = getPlayerChunk();
        List<ChunkProperties> chunksToUnload = new List<ChunkProperties>();
        for (int x = chunkList.MinX; x < playerChunk.x-chunkUnloadRange; x ++) {
            for (int y = chunkList.MinY; y <= chunkList.MaxX; y ++) {
                if (chunkList.inChunkBoundary(x,y) && chunkList.GetChunk(x,y) != null && chunkList.GetChunk(x,y).FullLoaded) {
                    chunksToUnload.Add(chunkList.GetChunk(x,y));
                }
            }
        }
        for (int x = playerChunk.x + chunkUnloadRange+1; x <= chunkList.MaxX; x ++) {
            for (int y = chunkList.MinY; y <= chunkList.MaxX; y ++) {
                if (chunkList.inChunkBoundary(x,y) && chunkList.GetChunk(x,y) != null && chunkList.GetChunk(x,y).FullLoaded) {
                    chunksToUnload.Add(chunkList.GetChunk(x,y));
                }
            }
        }
        for (int x = playerChunk.x - chunkUnloadRange; x < playerChunk.x + chunkUnloadRange; x ++) {
            for (int y = chunkList.MinY; y < playerChunk.y - chunkUnloadRange; y ++) {
                if (chunkList.inChunkBoundary(x,y) && chunkList.GetChunk(x,y) != null && chunkList.GetChunk(x,y).FullLoaded) {
                    chunksToUnload.Add(chunkList.GetChunk(x,y));
                }
            }
        }
        for (int x = playerChunk.x - chunkUnloadRange; x < playerChunk.x + chunkUnloadRange; x ++) {
            for (int y = playerChunk.y + chunkUnloadRange+1; y <= chunkList.MaxY; y ++) {
                if (chunkList.inChunkBoundary(x,y) && chunkList.GetChunk(x,y) != null && chunkList.GetChunk(x,y).FullLoaded) {
                    chunksToUnload.Add(chunkList.GetChunk(x,y));
                }
            }
        }
        return chunksToUnload;
        
    }

    protected void unloadChunksFarFromPlayer() {
        List<ChunkProperties> chunksToUnload = new List<ChunkProperties>();
        Vector2Int playerChunk = getPlayerChunk();
        for (int x = chunkList.MinX; x < playerChunk.x-Global.ChunkLoadRange; x ++) {
            for (int y = chunkList.MinY; y <= chunkList.MaxX; y ++) {
                if (chunkList.inChunkBoundary(x,y)) {
                    chunkList.GetChunk(x,y).unfullLoadChunk();
                }
            }
        }
        for (int x = playerChunk.x + Global.ChunkLoadRange+1; x <= chunkList.MaxX; x ++) {
            for (int y = chunkList.MinY; y <= chunkList.MaxX; y ++) {
                if (chunkList.inChunkBoundary(x,y)) {
                    chunkList.GetChunk(x,y).unfullLoadChunk();
                }
            }
        }
        for (int x = playerChunk.x - Global.ChunkLoadRange; x < playerChunk.x + Global.ChunkLoadRange; x ++) {
            for (int y = chunkList.MinY; y < playerChunk.y - Global.ChunkLoadRange; y ++) {
                if (chunkList.inChunkBoundary(x,y)) {
                    chunkList.GetChunk(x,y).unfullLoadChunk();
                }
            }
        }
        for (int x = playerChunk.x - Global.ChunkLoadRange; x < playerChunk.x + Global.ChunkLoadRange; x ++) {
            for (int y = playerChunk.y + Global.ChunkLoadRange+1; y <= chunkList.MaxY; y ++) {
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
