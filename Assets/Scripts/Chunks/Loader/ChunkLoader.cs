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
    public Queue<Pos2D> loadQueue;

    void Awake()
    {
        ClosedChunkSystem[] closedChunkSystems = transform.parent.GetComponents<ClosedChunkSystem>();
        if (closedChunkSystems.Length > 1) {
            Debug.LogError("ChunkUnloader belongs to multiple dimensions");
        }
        loadQueue = new Queue<Pos2D>();
        closedChunkSystem = closedChunkSystems[0];
        StartCoroutine(load());
    }

    public void addToQueue(List<Pos2D> chunkPositionToLoad) {
        activeCoroutines += chunkPositionToLoad.Count;
        Pos2D playerChunkPosition = closedChunkSystem.getPlayerChunk();
        foreach (Pos2D vect in chunkPositionToLoad) {
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
            Pos2D playerChunkPosition = closedChunkSystem.getPlayerChunk();
            Pos2D closestPosition = loadQueue.Dequeue();
            if (closedChunkSystem.chunkIsCached(closestPosition)) {
                activeCoroutines--;
                continue;
            }
            cacheChunk(closestPosition);
            yield return new WaitForSeconds(delay);
        }
    }

    private void cacheChunk(Pos2D closestPosition) {
        ChunkIO.getChunkFromJson(closestPosition, closedChunkSystem);

        activeCoroutines--;
    }
}
