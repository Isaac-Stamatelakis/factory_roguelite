using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Chunks.Systems;
using Chunks.Partitions;

namespace Chunks.Loaders {
    public class PartitionUnloader : QueueUpdater<IChunkPartition>
    {
        private PartitionLoader loader;
        public void setLoader(PartitionLoader loader) {
            this.loader = loader;
        }
        public override bool canUpdate(IChunkPartition value, Vector2Int playerPosition)
        {
            value.SetScheduleForUnloading(false);
            return value.GetLoaded() && !value.InRange(playerPosition,CameraView.ChunkPartitionLoadRange.x,CameraView.ChunkPartitionLoadRange.y);
        }

        public override void update(IChunkPartition value)
        {
            value.SetTileLoaded(false);
            StartCoroutine(closedChunkSystem.unloadChunkPartition(value));
        }

        public override Vector2Int getPlayerPosition()
        {
            return closedChunkSystem.getPlayerChunkPartition();
        }
    }
}