using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Chunks.Systems;
using Chunks.Partitions;

namespace Chunks.Loaders {

    public class PartitionLoader : QueueUpdater<IChunkPartition>
    {
        private Vector2Int lastPlayerPartition;
        private Vector2Int positionChange;
        public override bool canUpdate(IChunkPartition value, Vector2Int playerPosition)
        {
            return !value.GetLoaded();
        }

        public override Vector2Int getPlayerPosition()
        {
            return closedChunkSystem.GetPlayerChunkPartition();
        }

        public override void update(IChunkPartition value)
        {
            StartCoroutine(closedChunkSystem.LoadChunkPartition(value));
        }
    }
}



