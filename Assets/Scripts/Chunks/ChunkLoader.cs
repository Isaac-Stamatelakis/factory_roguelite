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
    public float delay = 0.08f;
    [SerializeField]
    public int rapidUploadThreshold = 6;
    [SerializeField]
    public int uploadAmountThreshold = 2;
    [SerializeField]
    public int activeCoroutines = 0;
    public bool Activated {get{return activeCoroutines != 0;}}

    void Start()
    {

        ClosedChunkSystem[] closedChunkSystems = transform.parent.GetComponents<ClosedChunkSystem>();
        if (closedChunkSystems.Length > 1) {
            Debug.LogError("ChunkUnloader belongs to multiple dimensions");
        }
        closedChunkSystem = closedChunkSystems[0];
    }
    public IEnumerator load(List<ChunkProperties> chunksToLoad) {
        activeCoroutines += chunksToLoad.Count;
        while (chunksToLoad.Count > 0) {
            int loadAmount = chunksToLoad.Count/uploadAmountThreshold+1;
            Vector2 playerChunkPosition = closedChunkSystem.getPlayerChunk();
            ChunkProperties closestChunk = chunksToLoad[0];
            for (int n = 1; n < chunksToLoad.Count; n++) {
                if 
                (
                    Mathf.Pow(playerChunkPosition.x-chunksToLoad[n].ChunkPosition.x,2) + Mathf.Pow(playerChunkPosition.y-chunksToLoad[n].ChunkPosition.y,2)
                    < 
                    Mathf.Pow(playerChunkPosition.x-closestChunk.ChunkPosition.x,2) + Mathf.Pow(playerChunkPosition.y-closestChunk.ChunkPosition.y,2)
                ) {
                    closestChunk = chunksToLoad[n];
                }
            }
            chunksToLoad.Remove(closestChunk);
            StartCoroutine(loadChunk(closestChunk,loadAmount));
            if (loadAmount*uploadAmountThreshold >= rapidUploadThreshold) { // Instant upload after threshhold reached
                yield return new WaitForEndOfFrame();
            } else { // upload speed increases rapidally if there are many chunks to upload
                yield return new WaitForSeconds(this.delay/uploadAmountThreshold);   
            }
        }
    }

    private IEnumerator loadChunk(ChunkProperties chunk, int loadAmount) {
        if (loadAmount < rapidUploadThreshold) {
                yield return chunk.fullLoadChunk(sectionAmount:16,Vector2Int.zero);
            } else {
                yield return chunk.fullLoadChunk(sectionAmount:loadAmount,Vector2Int.zero);
        }
        activeCoroutines--;
    }
}

