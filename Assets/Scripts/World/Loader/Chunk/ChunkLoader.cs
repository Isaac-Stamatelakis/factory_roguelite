using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.IO;
using Chunks.Systems;

namespace Chunks.Loaders {
    public class ChunkLoader : QueueUpdater<Vector2Int>
    {
        public override bool canUpdate(Vector2Int position,Vector2Int playerPosition)
        {
            return !closedChunkSystem.chunkIsCached(position);
        }

        public override Vector2Int getPlayerPosition()
        {
            return closedChunkSystem.getPlayerChunk();
        }

        public override void update(Vector2Int position)
        {
            closedChunkSystem.cacheChunk(position);
        }
    }
}

