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
            return closedChunkSystem.getPlayerChunkPartition();
        }

        public override void update(IChunkPartition value)
        {
            value.SetTileLoaded(true);
            Vector2Int playerPosition = getPlayerPosition();
            Vector2Int position = value.GetRealPosition();
            Vector2Int positionChange = closedChunkSystem.getPlayerPartitionChangeDifference();
            Direction loadDirection = Direction.Left;
            if (positionChange.x < 0) {
                loadDirection = Direction.Left;
            } else if (positionChange.x > 0) {
                loadDirection = Direction.Right;
            } else if (positionChange.y < 0) {
                loadDirection = Direction.Down;
            } else if (positionChange.y > 0) {
                loadDirection = Direction.Up;
            }
            
            StartCoroutine(closedChunkSystem.loadChunkPartition(value,loadDirection));
        }
    }
}



