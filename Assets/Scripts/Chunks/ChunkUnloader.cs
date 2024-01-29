using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class ChunkUnloader : MonoBehaviour
{
    private ChunkLoader chunkLoader;
    public ChunkLoader Loader {set{chunkLoader = value;}}
    [SerializeField]
    public float delay = 0.05F;
    [SerializeField]
    public int rapidDeleteThreshold = 100000;
    public int speedIncreaseThreshold = 200;
    [SerializeField]
    public int activeCoroutines = 0;
    private ClosedChunkSystem closedChunkSystem;
    Queue<Chunk> chunkQueue = new Queue<Chunk>();
    // Start is called before the first frame update
    void Start()
    {

        ClosedChunkSystem[] closedChunkSystems = transform.parent.GetComponents<ClosedChunkSystem>();
        if (closedChunkSystems.Length > 1) {
            Debug.LogError("ChunkUnloader belongs to multiple dimensions");
        }
        closedChunkSystem = closedChunkSystems[0];
        StartCoroutine(unload());
    }
    /// <summary>
    /// unloads the furthest chunk from the player at an interval
    /// </summary>

    public void addToQueue(List<Chunk> chunksToUnload) {
        activeCoroutines += chunksToUnload.Count;
        Vector2Int playerChunkPosition = closedChunkSystem.getPlayerChunk();
        chunksToUnload.Sort((a, b) => distance(playerChunkPosition,a).CompareTo(distance(playerChunkPosition, b)));
        for (int j =0;j < chunksToUnload.Count; j++) {
            Chunk chunkProperties = chunksToUnload[j];
            chunkProperties.ScheduledForUnloading = true;
            chunkQueue.Enqueue(chunkProperties);
             
        }
        chunksToUnload.Clear();
    }
    public IEnumerator unload() {
        while (true) {
            if (chunkQueue.Count == 0) {
                yield return new WaitForSeconds(delay);
                continue;
            }
            if (chunkLoader.Activated && chunkQueue.Count < speedIncreaseThreshold) {
                yield return new WaitForSeconds(delay);
                continue;
            }
            Chunk farthestChunk = chunkQueue.Dequeue();
            Vector2Int playerChunkPosition = closedChunkSystem.getPlayerChunk();
            if (distance(playerChunkPosition,farthestChunk) < (Global.ChunkLoadRangeX+2)*(Global.ChunkLoadRangeX+2)) {
                activeCoroutines--;
                farthestChunk.ScheduledForUnloading = false;
                continue;
            }
            int speedIncrease = chunkQueue.Count/speedIncreaseThreshold+1;
            if (chunkQueue.Count >= rapidDeleteThreshold) { // Instant removal after threshhold reached
                StartCoroutine(unloadChunk(farthestChunk,true));
                yield return new WaitForEndOfFrame();
                
            } else { // 
                StartCoroutine(unloadChunk(farthestChunk,false));
                yield return new WaitForSeconds(this.delay/speedIncrease);
            }
            
        }
    }

    public IEnumerator unloadChunk(Chunk chunk, bool fast) {
        if (fast) {
            chunk.instantlyUnFullLoadChunk(); 
            //yield return chunk.unfullLoadChunk();
        } else {
            yield return chunk.unfullLoadChunk();
        }
        activeCoroutines--;
    }

    public List<Chunk> getMax(List<Chunk> chunksToUnload,int m,Vector2Int playerPosition) {
        if (chunksToUnload.Count < m)
        {
           return chunksToUnload;
        }

        List<Chunk> furthestChunks = chunksToUnload.Take(m).ToList();
        foreach (Chunk chunk in chunksToUnload.Skip(m))
        {
            double dist = distance(playerPosition, chunk);
            if (dist > distance(playerPosition, furthestChunks[m-1]))
            {
                furthestChunks[m-1] = chunk;
                furthestChunks.Sort((a, b) => distance(playerPosition, b).CompareTo(distance(playerPosition, a)));
            }
        }

        return furthestChunks;
    }

    private double distance(Vector2Int playerPosition,Chunk chunk) {
        return Mathf.Pow(playerPosition.x-chunk.ChunkPosition.x,2) + Mathf.Pow(playerPosition.y-chunk.ChunkPosition.y,2);
    }
}
