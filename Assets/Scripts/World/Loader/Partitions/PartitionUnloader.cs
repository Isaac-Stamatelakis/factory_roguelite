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
            value.setScheduleForUnloading(false);
            return value.getLoaded() && !value.inRange(playerPosition,Global.ChunkPartitionLoadRange.x,Global.ChunkPartitionLoadRange.y);
        }

        public override void update(IChunkPartition value)
        {
            value.setTileLoaded(false);
            StartCoroutine(closedChunkSystem.unloadChunkPartition(value));
        }

        public override Vector2Int getPlayerPosition()
        {
            return closedChunkSystem.getPlayerChunkPartition();
        }
    }
}