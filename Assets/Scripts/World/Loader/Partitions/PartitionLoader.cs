using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Chunks.Systems;
using Chunks.Partitions;

namespace Chunks.Loaders {
    public enum PartitionQueue {
        Standard,
        Far
    }
    public class PartitionLoader : QueueUpdater<IChunkPartition>
    {
        public override bool canUpdate(IChunkPartition value, Vector2Int playerPosition)
        {
            return !value.getLoaded();
        }

        public override Vector2Int getPlayerPosition()
        {
            return closedChunkSystem.getPlayerChunkPartition();
        }

        public override void update(IChunkPartition value)
        {
            value.setTileLoaded(true);
            Vector2Int playerPosition = getPlayerPosition();
            Vector2Int position = value.getRealPosition();
            if (playerPosition.x > position.x) {

            } else {

            }
            StartCoroutine(closedChunkSystem.loadChunkPartition(value,Vector2Int.zero));
        }
    }
}



