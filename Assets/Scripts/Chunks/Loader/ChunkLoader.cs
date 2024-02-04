using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    [SerializeField]
    public bool active = false;
    private ClosedChunkSystem closedChunkSystem;
    [SerializeField]
    public float delay = 0.05F;
    [SerializeField]
    public int rapidUploadThreshold = 6;
    [SerializeField]
    public int uploadAmountThreshold = 2;
    [SerializeField]
    public int activeCoroutines = 0;
    public bool Activated {get{return activeCoroutines != 0;}}
    public Queue<Vector2Int> loadQueue;

    void Awake()
    {
        ClosedChunkSystem[] closedChunkSystems = transform.parent.GetComponents<ClosedChunkSystem>();
        if (closedChunkSystems.Length > 1) {
            Debug.LogError("ChunkUnloader belongs to multiple dimensions");
        }
        loadQueue = new Queue<Vector2Int>();
        closedChunkSystem = closedChunkSystems[0];
        StartCoroutine(load());
    }

    public void addToQueue(List<Vector2Int> chunkPositionToLoad) {
        activeCoroutines += chunkPositionToLoad.Count;
        Vector2Int playerChunkPosition = closedChunkSystem.getPlayerChunk();
        foreach (Vector2Int vect in chunkPositionToLoad) {
            loadQueue.Enqueue(vect);
        }
        
        //chunkPositionToLoad.Sort((a, b) => b.distanceFrom(playerChunkPosition).CompareTo(a.distanceFrom(playerChunkPosition)));
        //for (int j =0 ;j < chunkPositionToLoad.Count; j++) {
         //   loadQueue.Enqueue(chunkPositionToLoad[j]);
        //}
        chunkPositionToLoad.Clear();
        
    }
    public IEnumerator load() {
        while (true) {
            if (loadQueue.Count == 0) {
                yield return new WaitForSeconds(this.delay);
                continue;
            }
            int loadAmount = activeCoroutines/uploadAmountThreshold+1;
            Vector2Int playerChunkPosition = closedChunkSystem.getPlayerChunk();
            Vector2Int closestPosition = loadQueue.Dequeue();
            if (closedChunkSystem.chunkIsCached(closestPosition)) {
                activeCoroutines--;
                continue;
            }
            cacheChunk(closestPosition);
            yield return new WaitForSeconds(delay);
        }
    }

    private void cacheChunk(Vector2Int closestPosition) {
        ChunkIO.getChunkFromJson(closestPosition, closedChunkSystem);
        activeCoroutines--;
    }
}
