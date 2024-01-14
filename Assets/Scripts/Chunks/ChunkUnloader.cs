using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkUnloader : MonoBehaviour
{

    private float delay = 1f;
    private ClosedChunkSystem closedChunkSystem;
    // Start is called before the first frame update
    void Start()
    {

        ClosedChunkSystem[] closedChunkSystems = transform.parent.GetComponents<ClosedChunkSystem>();
        if (closedChunkSystems.Length > 1) {
            Debug.LogError("ChunkUnloader belongs to multiple dimensions");
        }
        closedChunkSystem = closedChunkSystems[0];
        this.StartCoroutine(unload());

    }
    /// <summary>
    /// unloads the furthest chunk from the player at an interval
    /// </summary>
    public IEnumerator unload() {
        while (true) {
            List<ChunkProperties> chunksToUnload = closedChunkSystem.getLoadedChunksFar();
            if (chunksToUnload.Count > 0) {
                Vector2 playerChunkPosition = closedChunkSystem.getPlayerChunk();
                ChunkProperties farthestChunk = chunksToUnload[0];
                for (int n = 1; n < chunksToUnload.Count; n++) {
                    if 
                    (
                        Mathf.Pow(playerChunkPosition.x-chunksToUnload[n].ChunkPosition.x,2) + Mathf.Pow(playerChunkPosition.y-chunksToUnload[n].ChunkPosition.y,2)
                        > 
                        Mathf.Pow(playerChunkPosition.x-farthestChunk.ChunkPosition.x,2) + Mathf.Pow(playerChunkPosition.y-farthestChunk.ChunkPosition.y,2)
                    ) {
                        farthestChunk = chunksToUnload[n];
                    }
                }
                farthestChunk.unfullLoadChunk();
            }
            yield return new WaitForSeconds(this.delay);
        }
    }
}
