using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class ChunkUnloader : MonoBehaviour
{
    private ChunkLoader chunkLoader;
    public ChunkLoader Loader {set{chunkLoader = value;}}
    [SerializeField]
    public float delay = 0.5f;
    [SerializeField]
    public int rapidDeleteThreshold = 75;
    [SerializeField]
    public int removalAmountThreshold = 15;
    [SerializeField]
    public int activeCoroutines = 0;
    private ClosedChunkSystem closedChunkSystem;
    // Start is called before the first frame update
    void Start()
    {

        ClosedChunkSystem[] closedChunkSystems = transform.parent.GetComponents<ClosedChunkSystem>();
        if (closedChunkSystems.Length > 1) {
            Debug.LogError("ChunkUnloader belongs to multiple dimensions");
        }
        closedChunkSystem = closedChunkSystems[0];
    }
    /// <summary>
    /// unloads the furthest chunk from the player at an interval
    /// </summary>
    public IEnumerator unload(List<ChunkProperties> chunksToUnload) {
        activeCoroutines += chunksToUnload.Count;
        Vector2Int playerChunkPosition = closedChunkSystem.getPlayerChunk();
        chunksToUnload.Sort((a, b) => distance(playerChunkPosition,b).CompareTo(distance(playerChunkPosition, a)));
        while (chunksToUnload.Count > 0) {
            if (chunkLoader.Activated) {
                yield return new WaitForSeconds(delay*10);
                continue;
            }
            int removalAmount = activeCoroutines/removalAmountThreshold+1;
            ChunkProperties farthestChunk = chunksToUnload[0];
            chunksToUnload.Remove(farthestChunk);
            StartCoroutine(unloadChunk(farthestChunk,removalAmount < rapidDeleteThreshold));
            if (removalAmount*removalAmountThreshold >= rapidDeleteThreshold) { // Instant removal after threshhold reached
                yield return new WaitForEndOfFrame();
            } else { // Removal speed increases rapidally if there are many chunks to delete
                yield return new WaitForSeconds(this.delay/removalAmount);
            }
            
        }
    }

    public IEnumerator unloadChunk(ChunkProperties chunk, bool fast) {
        if (fast) {
            chunk.instantlyUnFullLoadChunk(); 
        } else {
            yield return chunk.unfullLoadChunk();
        }
        activeCoroutines--;
        

    }

    public List<ChunkProperties> getMax(List<ChunkProperties> chunksToUnload,int m,Vector2Int playerPosition) {
        if (chunksToUnload.Count < m)
        {
           return chunksToUnload;
        }

        List<ChunkProperties> furthestChunks = chunksToUnload.Take(m).ToList();
        foreach (ChunkProperties chunk in chunksToUnload.Skip(m))
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

    private double distance(Vector2Int playerPosition,ChunkProperties chunk) {
        return Mathf.Pow(playerPosition.x-chunk.ChunkPosition.x,2) + Mathf.Pow(playerPosition.y-chunk.ChunkPosition.y,2);
    }
}
