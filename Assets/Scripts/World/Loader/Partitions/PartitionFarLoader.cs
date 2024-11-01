using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.Partitions;

namespace Chunks.Loaders {
    public class PartitionFarLoader : QueueUpdater<IChunkPartition>
    {
        public override bool canUpdate(IChunkPartition value, Vector2Int playerPosition)
        {
            if (value.getFarLoaded()) {
                value.setScheduledForFarLoading(false);
                return false;
            }
            return true;
        }

        public override Vector2Int getPlayerPosition()
        {
            return closedChunkSystem.getPlayerChunkPartition();
        }

        public override void update(IChunkPartition value)
        {
            value.loadFarLoadTileEntities();
        }
    }

}
