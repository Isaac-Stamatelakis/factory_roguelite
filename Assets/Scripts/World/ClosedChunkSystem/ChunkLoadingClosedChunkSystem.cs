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

        protected IEnumerator initalLoadChunks()
        {
            List<Vector2Int> chunks = GetUnCachedChunkPositionsNearPlayer();
            foreach (Vector2Int vector in chunks) {
                addChunk(ChunkIO.GetChunkFromJson(vector, this));
            }
            yield return null;
            Debug.Log("Chunks Near Player Loaded");
        }

        protected IEnumerator initalLoad() {
            yield return StartCoroutine(initalLoadChunks());
            PlayerPartitionUpdate();
            Debug.Log("Partitions Near Player Loaded");
            yield return new WaitForSeconds(1f);
            Debug.Log("Player Activated");
        }

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

