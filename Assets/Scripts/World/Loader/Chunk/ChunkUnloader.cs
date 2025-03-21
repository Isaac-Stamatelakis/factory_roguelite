using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Chunks.Systems;

namespace Chunks.Loaders {
    public class ChunkUnloader : QueueUpdater<Chunk>
    {
        public override bool canUpdate(Chunk value, Vector2Int playerPosition)
        {
            value.ScheduleForUnloading = false;
            return closedChunkSystem.ChunkIsCached(value.GetPosition()) && value.PartionsAreAllUnloaded();
        }

        public override Vector2Int getPlayerPosition()
        {
            return closedChunkSystem.GetPlayerChunk();
        }

        public override void update(Chunk value)
        {
            closedChunkSystem.RemoveChunk(value);
            value.Unload();
        }
    }

}
