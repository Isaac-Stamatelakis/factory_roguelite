using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.Loaders;
using Chunks.IO;

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
            chunkLoader.initalize(this,LoadUtils.getChunkLoaderVariables());

            chunkUnloader = ChunkContainerTransform.gameObject.AddComponent<ChunkUnloader>();
            chunkUnloader.initalize(this,LoadUtils.getChunkUnloaderVariables());
            base.InitLoaders();
        }

        public override void PlayerChunkUpdate()
        {
            chunkLoader.addToQueue(GetUnCachedChunkPositionsNearPlayer());
            chunkUnloader.addToQueue(GetLoadedChunksOutsideRange());
        
        }
    }
}

