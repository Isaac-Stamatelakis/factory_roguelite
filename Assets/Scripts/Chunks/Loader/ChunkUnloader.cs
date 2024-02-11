using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule;
public class ChunkUnloader : MonoBehaviour
{
    [SerializeField]
    public bool active = false;
    private ClosedChunkSystem closedChunkSystem;
    [SerializeField]
    public float delay = 1F; // 1
    [SerializeField]
    public int rapidUploadThreshold = 10000;
    [SerializeField]
    public int uploadAmountThreshold = 1000;
    [SerializeField]
    public int activeCoroutines = 0;
    public bool Activated {get{return activeCoroutines != 0;}}
    public Queue<Chunk> unloadQueue;
    public void init(ClosedChunkSystem closedChunkSystem) {
        this.closedChunkSystem = closedChunkSystem;
        unloadQueue = new Queue<Chunk>();
        StartCoroutine(unload());
    }

    public void addToQueue(List<Chunk> chunksToUnLoad) {
        activeCoroutines += chunksToUnLoad.Count;
        Vector2Int playerChunkPosition = closedChunkSystem.getPlayerChunk();
        
        
        chunksToUnLoad.Sort((a, b) => b.distanceFrom(playerChunkPosition).CompareTo(a.distanceFrom(playerChunkPosition)));
        for (int j =0 ;j < chunksToUnLoad.Count; j++) {
            unloadQueue.Enqueue(chunksToUnLoad[j]);
        }
        chunksToUnLoad.Clear();
        
    }
    public IEnumerator unload() {
        while (true) {
            if (unloadQueue.Count == 0) {
                yield return new WaitForSeconds(this.delay);
                continue;
            }
            int loadAmount = activeCoroutines/uploadAmountThreshold+1;
            Chunk farthestChunk = unloadQueue.Dequeue();
            activeCoroutines--;
            if (farthestChunk != null && closedChunkSystem.chunkIsCached(farthestChunk.getPosition()) && farthestChunk.partionsAreAllUnloaded()) {
                closedChunkSystem.removeChunk(farthestChunk);
                farthestChunk.unload();
                yield return new WaitForSeconds(delay);
            } else {
                yield return new WaitForEndOfFrame();
            }
            
            
        }
    }
}
