using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Chunks.Loaders;
using Chunks.IO;
using Chunks.Partitions;
using Fluids;

namespace Chunks.Systems {
    /// <summary>
    /// Chunks in this closed chunk system load and unload depending on player position
    /// </summary>
    
    public class ChunkLoadingClosedChunkSystem : ClosedChunkSystem
    {
        protected ChunkLoader chunkLoader;
        protected ChunkUnloader chunkUnloader;
        public override void InitLoaders()
        {
            chunkLoader = chunkContainerTransform.gameObject.AddComponent<ChunkLoader>();
            chunkLoader.Initalize(this,LoadUtils.getChunkLoaderVariables());

            chunkUnloader = ChunkContainerTransform.gameObject.AddComponent<ChunkUnloader>();
            chunkUnloader.Initalize(this,LoadUtils.getChunkUnloaderVariables());
            base.InitLoaders();
        }

        public override void PlayerChunkUpdate()
        {
            chunkLoader.addToQueue(GetUnCachedChunkPositionsNearPlayer());
            chunkUnloader.addToQueue(GetLoadedChunksOutsideRange());
        
        }

        public override void TickUpdate()
        {
            foreach (ILoadedChunk chunk in cachedChunks.Values) {
                foreach (IChunkPartition partition in chunk.GetChunkPartitions()) {
                    partition.Tick();
                }
            }
        }

        public override IEnumerator SaveCoroutine()
        {
            var delay = new WaitForFixedUpdate();
            FluidTileMap fluidTileMap = GetFluidTileMap();
            List<Vector2Int> currentlyCachedChunks = cachedChunks.Keys.ToList(); // Required to prevent modifying collection during enumeration
            foreach (Vector2Int chunkPosition in currentlyCachedChunks) {
                if (!cachedChunks.TryGetValue(chunkPosition, out var chunk)) continue;
                fluidTileMap?.Simulator.SaveToChunk(chunk);
                foreach (IChunkPartition partition in chunk.GetChunkPartitions()) {
                    if (partition.GetLoaded()) {
                        partition.Save();
                    }
                }
                ChunkIO.WriteChunk(chunk);
                yield return delay;
            }
        }
    }
}

