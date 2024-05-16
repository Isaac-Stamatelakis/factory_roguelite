using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Chunks.ClosedChunkSystemModule;
using Chunks.Partitions;

namespace Chunks.LoadController {
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

        public void init(ClosedChunkSystem closedChunkSystem) {
            this.closedChunkSystem = closedChunkSystem;
            loadQueue = new Queue<IChunkPartition>();
            StartCoroutine(load());
        }

        public void addToQueue(List<IChunkPartition> partitionsToLoad) {
            activeCoroutines += partitionsToLoad.Count;
            if (closedChunkSystem == null) {

            }
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
                    closestPartition.setTileLoaded(true);
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
}


