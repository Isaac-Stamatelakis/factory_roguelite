using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Chunks.Systems;
using Chunks.Partitions;

namespace Chunks.LoadController {
    public enum PartitionQueue {
        Standard,
        Far
    }
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
        public Queue<IChunkPartition> farQueue;

        public void init(ClosedChunkSystem closedChunkSystem) {
            this.closedChunkSystem = closedChunkSystem;
            loadQueue = new Queue<IChunkPartition>();
            farQueue = new Queue<IChunkPartition>();
            StartCoroutine(load());
            StartCoroutine(loadFar());
        }

        public void addToQueue(List<IChunkPartition> partitionsToLoad, PartitionQueue partitionQueue) {
            if (partitionQueue == PartitionQueue.Standard) {
                activeCoroutines += partitionsToLoad.Count;
            }
            Queue<IChunkPartition> partitions = null;
            switch (partitionQueue) {
                case PartitionQueue.Standard:
                    partitions = loadQueue;
                    break;
                case PartitionQueue.Far:
                    partitions = farQueue;
                    break;
            }
            Vector2Int playerChunkPosition = closedChunkSystem.getPlayerChunkPartition();
            partitionsToLoad.Sort((a, b) => b.distanceFrom(playerChunkPosition).CompareTo(a.distanceFrom(playerChunkPosition)));
            for (int j =0 ;j < partitionsToLoad.Count; j++) {
                partitions.Enqueue(partitionsToLoad[j]);
            }
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

        public IEnumerator loadFar() {
            while (true) {
                if (farQueue.Count == 0) {
                    yield return new WaitForSeconds(this.delay);
                    continue;
                }
                int loadAmount = 1;
                while (loadAmount > 0 && farQueue.Count != 0) {
                    IChunkPartition closestPartition = farQueue.Dequeue();
                    if (closestPartition.getFarLoaded()) {
                        continue;
                    }
                    closestPartition.loadFarLoadTileEntities();
                    
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


