using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.LoadController;
using Chunks.IO;

namespace Chunks.ClosedChunkSystemModule {
    /// <summary>
    /// Chunks in this closed chunk system load and unload depending on player position
    /// </summary>
    
    public class ChunkLoadingClosedChunkSystem : ClosedChunkSystem
    {
        protected ChunkLoader chunkLoader;
        protected ChunkUnloader chunkUnloader;

        protected IEnumerator initalLoadChunks()
        {
            List<Vector2Int> chunks = getUnCachedChunkPositionsNearPlayer();
            foreach (Vector2Int vector in chunks) {
                addChunk(ChunkIO.getChunkFromJson(vector, this));
            }
            yield return null;
            Debug.Log("Chunks Near Player Loaded");
        }

        protected IEnumerator initalLoad() {
            yield return StartCoroutine(initalLoadChunks());
            playerPartitionUpdate();
            Debug.Log("Partitions Near Player Loaded");
            yield return new WaitForSeconds(1f);
            Debug.Log("Player Activated");
        }

        public override void initLoaders()
        {
            chunkLoader = chunkContainerTransform.gameObject.AddComponent<ChunkLoader>();
            chunkLoader.init(this);

            chunkUnloader = ChunkContainerTransform.gameObject.AddComponent<ChunkUnloader>();
            chunkUnloader.init(this);
            base.initLoaders();
        }

        public override void playerChunkUpdate()
        {
            chunkLoader.addToQueue(getUnCachedChunkPositionsNearPlayer());
            chunkUnloader.addToQueue(getLoadedChunksOutsideRange());
        
        }
    }
}

