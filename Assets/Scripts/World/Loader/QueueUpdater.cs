using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.Systems;

namespace Chunks.Loaders {

    public enum QueueSortingMode {
        None,
        Closest,
        Farthest
    }
    [System.Serializable]
    public struct QueueUpdateVariables {
        public float Delay;
        public int RapidUpdateThreshold;
        public int RapidSpeed;
        public int BaseBatchSize;
        public QueueSortingMode SortingMode;

        public QueueUpdateVariables(float delay, int rapidUpdateThreshold, int rapidUpdateSpeed, int baseBatchSize, QueueSortingMode sortingMode)
        {
            this.SortingMode = sortingMode;
            this.Delay = delay;
            RapidUpdateThreshold = rapidUpdateThreshold;
            RapidSpeed = rapidUpdateSpeed;
            BaseBatchSize = baseBatchSize;
        }
    }
    public abstract class QueueUpdater<T> : MonoBehaviour
    {
        [SerializeField] protected QueueUpdateVariables variables;
        protected ClosedChunkSystem closedChunkSystem;
        protected Queue<T> updateQueue;
        public int QueueSize;
        private float timeSinceLastBatch;
        public bool Activated => updateQueue.Count > 0;

        public void initalize(ClosedChunkSystem closedChunkSystem, QueueUpdateVariables updateVariables) {
            this.closedChunkSystem = closedChunkSystem;
            updateQueue = new Queue<T>();
            this.variables = updateVariables;
        }

        public void clearQueue() {
            updateQueue.Clear();
        }
        public void addToQueue(List<T> chunkPositionToLoad) {
            QueueSize += chunkPositionToLoad.Count;
            Vector2Int playerChunkPosition = closedChunkSystem.getPlayerChunk();
            foreach (T vect in chunkPositionToLoad) {
                updateQueue.Enqueue(vect);
            }
        }

        public void FixedUpdate() {
            timeSinceLastBatch += Time.fixedDeltaTime;
            if (timeSinceLastBatch < variables.Delay) {
                return;
            }
            timeSinceLastBatch = 0;
            int updates = getBatchSize();
            Vector2Int playerPosition = getPlayerPosition();
            
            while (updateQueue.Count > 0 && updates > 0) {
                T updateElement = updateQueue.Dequeue();
                QueueSize--;
                if (!canUpdate(updateElement,playerPosition)) {
                    continue;
                }
                update(updateElement);
                updates--;
            }
        }
      
        public abstract bool canUpdate(T value, Vector2Int playerPosition);
        public abstract void update(T value);
        public abstract Vector2Int getPlayerPosition();
        public int getBatchSize() {
            return variables.BaseBatchSize + QueueSize/variables.RapidUpdateThreshold*variables.RapidSpeed;
        }
    }
}

