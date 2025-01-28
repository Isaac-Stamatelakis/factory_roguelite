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
            return value != null && closedChunkSystem.chunkIsCached(value.GetPosition()) && value.partionsAreAllUnloaded();
        }

        public override Vector2Int getPlayerPosition()
        {
            return closedChunkSystem.getPlayerChunk();
        }


        /*
public float delay = 1F; // 1
[SerializeField]
public int rapidUploadThreshold = 10000;
[SerializeField]
public int uploadAmountThreshold = 1000;
*/

        public override void update(Chunk value)
        {
            closedChunkSystem.removeChunk(value);
            value.unload();
        }
    }

}
