using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Chunks.Systems;
using Chunks.Partitions;

namespace Chunks.Loaders {
    public class PartitionUnloadFailUpdater : QueueFailUpdater<IChunkPartition>
    {
        public override void OnUpdateFail(IChunkPartition value)
        {
            
        }
    }
    public class PartitionUnloader : QueueUpdater<IChunkPartition>
    {
        public override void InitializeMiscUpdaters()
        {
            queueFailUpdater = new PartitionUnloadFailUpdater();
        }

        public override bool canUpdate(IChunkPartition value, Vector2Int playerPosition)
        {
            // If the partition is updated, it is no longer scheduled for unloading.
            // If the partition is not updated it is no longer scheduled for unloading.
            value.SetScheduleForUnloading(false);
            return value.GetLoaded() && !value.InRange(playerPosition,CameraView.ChunkPartitionLoadRange.x,CameraView.ChunkPartitionLoadRange.y);
        }

        public override void update(IChunkPartition value)
        {
            StartCoroutine(closedChunkSystem.UnloadChunkPartition(value));
        }

        public override Vector2Int getPlayerPosition()
        {
            return closedChunkSystem.GetPlayerChunkPartition();
        }
    }
}