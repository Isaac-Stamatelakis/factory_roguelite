using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    private ClosedChunkSystem closedChunkSystem;
    private float delay = 0.05f;

    void Start()
    {

        ClosedChunkSystem[] closedChunkSystems = transform.parent.GetComponents<ClosedChunkSystem>();
        if (closedChunkSystems.Length > 1) {
            Debug.LogError("ChunkUnloader belongs to multiple dimensions");
        }
        closedChunkSystem = closedChunkSystems[0];
    }
    public IEnumerator load(List<ChunkProperties> chunksToLoad) {
        while (chunksToLoad.Count > 0) {
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
            closestChunk.fullLoadChunk();
            chunksToLoad.Remove(closestChunk);
            yield return new WaitForSeconds(this.delay);
        }
        
        
    }
}

