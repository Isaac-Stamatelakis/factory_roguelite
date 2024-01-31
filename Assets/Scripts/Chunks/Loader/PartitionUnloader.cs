using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class PartitionUnloader : MonoBehaviour
{
    private PartitionLoader chunkLoader;
    public PartitionLoader Loader {set{chunkLoader = value;}}
    [SerializeField]
    public float delay = 0.05F;
    [SerializeField]
    public int rapidDeleteThreshold = 100000;
    public int speedIncreaseThreshold = 200;
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
        Pos2D playerChunkPosition = closedChunkSystem.getPlayerChunk();
        partitionsToUnload.Sort((a, b) => a.distanceFrom(playerChunkPosition).CompareTo(b.distanceFrom(playerChunkPosition)));
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
            IChunkPartition farthestPartition = unloadQueue.Dequeue();
            Pos2D playerChunkPosition = closedChunkSystem.getPlayerChunk();
            if (farthestPartition.distanceFrom(playerChunkPosition) < (Global.ChunkPartitionLoadRange.x+2)*(Global.ChunkPartitionLoadRange.x+2)) {
                activeCoroutines--;
                farthestPartition.setScheduleForUnloading(false);
                continue;
            }
            int speedIncrease = unloadQueue.Count/speedIncreaseThreshold+1;
            if (unloadQueue.Count >= rapidDeleteThreshold) { // Instant removal after threshhold reached
                StartCoroutine(unloadChunk(farthestPartition,true));
                yield return new WaitForEndOfFrame();
                
            } else { // 
                StartCoroutine(unloadChunk(farthestPartition,false));
                yield return new WaitForSeconds(this.delay/speedIncrease);
            }
            
        }
    }

    public IEnumerator unloadChunk(IChunkPartition partition, bool fast) {
        yield return closedChunkSystem.unloadChunkPartition(partition);
        activeCoroutines--;
    }

    private double distance(Vector2Int playerPosition,Chunk chunk) {
        return Mathf.Pow(playerPosition.x-chunk.ChunkPosition.x,2) + Mathf.Pow(playerPosition.y-chunk.ChunkPosition.y,2);
    }
}
