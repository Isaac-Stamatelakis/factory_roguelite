using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class PartitionUnloader : MonoBehaviour
{
    private PartitionLoader chunkLoader;
    public PartitionLoader Loader {set{chunkLoader = value;}}
    [SerializeField]
    public float delay = 0.05F; // 0.05F
    [SerializeField]
    public int rapidDeleteThreshold = 1600;
    public int speedIncreaseThreshold = 800; //200
    [SerializeField]
    public int activeCoroutines = 0;
    private ClosedChunkSystem closedChunkSystem;
    Queue<IChunkPartition> unloadQueue = new Queue<IChunkPartition>();
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

    public void addToQueue(List<IChunkPartition> partitionsToUnload) {
        activeCoroutines += partitionsToUnload.Count;
        Vector2Int playerChunkPosition = closedChunkSystem.getPlayerChunk();
        // Sort by distance from player
        partitionsToUnload.Sort((a, b) => a.distanceFrom(playerChunkPosition).CompareTo(b.distanceFrom(playerChunkPosition))); 

        // List is now sorted, enqueue into queue.
        for (int j =0;j < partitionsToUnload.Count; j++) {
            IChunkPartition partition = partitionsToUnload[j];
            partition.setScheduleForUnloading(true);
            unloadQueue.Enqueue(partition);
             
        }
        partitionsToUnload.Clear();
    }
    public IEnumerator unload() {
        while (true) {
            if (unloadQueue.Count == 0) {
                yield return new WaitForSeconds(delay);
                continue;
            }
            if (chunkLoader.Activated && unloadQueue.Count < speedIncreaseThreshold) {
                yield return new WaitForSeconds(delay);
                continue;
            }   
            
            int unloadAmount = unloadQueue.Count/speedIncreaseThreshold+1;
            unloadAmount = Mathf.Min(unloadAmount,2);
            while (unloadAmount > 0 && unloadQueue.Count != 0) {
                activeCoroutines--;
                IChunkPartition farthestPartition = unloadQueue.Dequeue();
                // If partition is close to not loaded or partition is to close to the player, do not unload
                if (!farthestPartition.getLoaded() || farthestPartition.inRange(closedChunkSystem.getPlayerChunkPartition(),Global.ChunkPartitionLoadRange.x+2,Global.ChunkPartitionLoadRange.y+2)) {
                    continue;
                }
                farthestPartition.setLoaded(false);
                StartCoroutine(closedChunkSystem.unloadChunkPartition(farthestPartition));
                unloadAmount --;
            }
            yield return new WaitForEndOfFrame();
            /*
            if (unloadQueue.Count >= rapidDeleteThreshold) { // Instant removal after threshhold reached
                yield return new WaitForEndOfFrame();
            } else {
                yield return new WaitForSeconds(this.delay);
            }
            */
            
        }
    }
}
