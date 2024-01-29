using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class ChunkLoader : MonoBehaviour
{
    [SerializeField]
    public bool active = false;
    private ClosedChunkSystem closedChunkSystem;
    [SerializeField]
    public float delay = 0.00f;
    [SerializeField]
    public int rapidUploadThreshold = 6;
    [SerializeField]
    public int uploadAmountThreshold = 2;
    [SerializeField]
    public int activeCoroutines = 0;
    public bool Activated {get{return activeCoroutines != 0;}}
    public Queue<Chunk> loadQueue;

    void Start()
    {

        ClosedChunkSystem[] closedChunkSystems = transform.parent.GetComponents<ClosedChunkSystem>();
        if (closedChunkSystems.Length > 1) {
            Debug.LogError("ChunkUnloader belongs to multiple dimensions");
        }
        loadQueue = new Queue<Chunk>();
        closedChunkSystem = closedChunkSystems[0];
        StartCoroutine(load());
    }

    public void addToQueue(List<Chunk> chunksToLoad) {
        activeCoroutines += chunksToLoad.Count;
        Vector2Int playerChunkPosition = closedChunkSystem.getPlayerChunk();
        chunksToLoad.Sort((a, b) => distance(playerChunkPosition,b).CompareTo(distance(playerChunkPosition, a)));
        for (int j =0 ;j < chunksToLoad.Count; j++) {
            loadQueue.Enqueue(chunksToLoad[j]);
        }
        chunksToLoad.Clear();
        
    }
    public IEnumerator load() {
        while (true) {
            if (loadQueue.Count == 0) {
                yield return new WaitForSeconds(this.delay);
                continue;
            }
            int loadAmount = activeCoroutines/uploadAmountThreshold+1;
            Vector2Int playerChunkPosition = closedChunkSystem.getPlayerChunk();
            Chunk closestChunk = loadQueue.Dequeue();
            if (closestChunk.FullLoaded) {
                activeCoroutines--;
                continue;
            }
            Vector2Int dif = playerChunkPosition-closestChunk.ChunkPosition;
            double angle = Mathf.Atan2(dif.y,dif.x);
            StartCoroutine(loadChunk(closestChunk,loadAmount,angle));
            
            if (loadAmount*uploadAmountThreshold >= rapidUploadThreshold) { // Instant upload after threshhold reached
                yield return new WaitForEndOfFrame();
            } else { // upload speed increases rapidally if there are many chunks to upload
                yield return new WaitForSeconds(this.delay/uploadAmountThreshold);   
            }
        }
    }

    private IEnumerator loadChunk(Chunk chunk, int loadAmount, double angle) {
        if (loadAmount < rapidUploadThreshold) {
                yield return chunk.fullLoadChunk(sectionAmount:16,angle);
            } else {
                yield return chunk.fullLoadChunk(sectionAmount:loadAmount,angle);
        }
        activeCoroutines--;
    }
    private double distance(Vector2Int playerPosition,Chunk chunk) {
        return Mathf.Pow(playerPosition.x-chunk.ChunkPosition.x,2) + Mathf.Pow(playerPosition.y-chunk.ChunkPosition.y,2);
    }
}

