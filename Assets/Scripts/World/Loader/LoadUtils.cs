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
                queueCapacity: 32
            );
        }
        public static QueueUpdateVariables getChunkUnloaderVariables() {
            return new QueueUpdateVariables(
                delay: 1f,
                rapidUpdateSpeed: 1,
                rapidUpdateThreshold: 50,
                baseBatchSize: 1,
                queueCapacity: 32
            );
        }
        public static QueueUpdateVariables getPartitionLoaderVariables() {
            return new QueueUpdateVariables(
                delay: 0.0f,
                rapidUpdateSpeed: 3,
                rapidUpdateThreshold: 6,
                baseBatchSize: 5,
                queueCapacity: 32
            );
        }
        
        public static QueueUpdateVariables getPartitionUnloaderVariables() {
            return new QueueUpdateVariables(
                delay: 0f,
                rapidUpdateSpeed: 2,
                rapidUpdateThreshold: 100,
                baseBatchSize: 2,
                queueCapacity: 338
            );
        }

        public static QueueUpdateVariables getPartitionFarLoaderVariables() {
            return new QueueUpdateVariables(
                delay: 0.0f,
                rapidUpdateSpeed: 3,
                rapidUpdateThreshold: 6,
                baseBatchSize: 2,
                queueCapacity: 32
            );
        }

        
    }

}
