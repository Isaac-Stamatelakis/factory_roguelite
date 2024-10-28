using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chunks.Loaders {
    public static class LoadUtils
    {
        public static QueueUpdateVariables getChunkLoaderVariables() {
            return new QueueUpdateVariables(
                delay: 0.05f,
                rapidUpdateSpeed: 25,
                rapidUpdateThreshold: 2,
                baseBatchSize: 1,
                sortingMode: QueueSortingMode.Closest
            );
        }
        public static QueueUpdateVariables getChunkUnloaderVariables() {
            return new QueueUpdateVariables(
                delay: 1f,
                rapidUpdateSpeed: 1,
                rapidUpdateThreshold: 100,
                baseBatchSize: 1,
                sortingMode: QueueSortingMode.Farthest
            );
        }
        public static QueueUpdateVariables getPartitionLoaderVariables() {
            return new QueueUpdateVariables(
                delay: 0.01f,
                rapidUpdateSpeed: 3,
                rapidUpdateThreshold: 6,
                baseBatchSize: 5,
                sortingMode: QueueSortingMode.Closest
            );
        }
        
        public static QueueUpdateVariables getPartitionUnloaderVariables() {
            return new QueueUpdateVariables(
                delay: 0f,
                rapidUpdateSpeed: 2,
                rapidUpdateThreshold: int.MaxValue,
                baseBatchSize: 2,
                sortingMode: QueueSortingMode.Farthest
            );
        }

        public static QueueUpdateVariables getPartitionFarLoaderVariables() {
            return new QueueUpdateVariables(
                delay: 0.05f,
                rapidUpdateSpeed: 3,
                rapidUpdateThreshold: 6,
                baseBatchSize: 2,
                sortingMode: QueueSortingMode.Closest
            );
        }

        
    }

}
