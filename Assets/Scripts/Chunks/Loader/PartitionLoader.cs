using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class PartitionLoader : MonoBehaviour
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
    public Queue<IChunkPartition> loadQueue;

    void Start()
    {

        ClosedChunkSystem[] closedChunkSystems = transform.parent.GetComponents<ClosedChunkSystem>();
        if (closedChunkSystems.Length > 1) {
            Debug.LogError("ChunkUnloader belongs to multiple dimensions");
        }
        loadQueue = new Queue<IChunkPartition>();
        closedChunkSystem = closedChunkSystems[0];
        StartCoroutine(load());
    }

    public void addToQueue(List<IChunkPartition> partitionsToLoad) {
        activeCoroutines += partitionsToLoad.Count;
        Vector2Int playerChunkPosition = closedChunkSystem.getPlayerChunk();
        partitionsToLoad.Sort((a, b) => b.distanceFrom(playerChunkPosition).CompareTo(a.distanceFrom(playerChunkPosition)));
        for (int j =0 ;j < partitionsToLoad.Count; j++) {
            loadQueue.Enqueue(partitionsToLoad[j]);
        }
        partitionsToLoad.Clear();
        
    }
    public IEnumerator load() {
        while (true) {
            if (loadQueue.Count == 0) {
                yield return new WaitForSeconds(this.delay);
                continue;
            }
            int loadAmount = activeCoroutines/uploadAmountThreshold+1;
            while (loadAmount > 0 && loadQueue.Count != 0) {
                Vector2Int playerChunkPosition = closedChunkSystem.getPlayerChunk();
                IChunkPartition closestPartition = loadQueue.Dequeue();
                if (closestPartition.getLoaded()) {
                    activeCoroutines--;
                    continue;
                }
                Vector2Int pos = closestPartition.getRealPosition();
                Vector2Int dif = new Vector2Int(playerChunkPosition.x-pos.x,playerChunkPosition.y-pos.y);
                double angle = Mathf.Rad2Deg*Mathf.Atan2(dif.y,dif.x)+180;
                closestPartition.setLoaded(true);
                StartCoroutine(loadChunkPartition(closestPartition,loadAmount,angle));
                loadAmount --;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator loadChunkPartition(IChunkPartition partition, int loadAmount, double angle) {
        yield return closedChunkSystem.loadChunkPartition(partition,angle);
        activeCoroutines--;
    }

}

