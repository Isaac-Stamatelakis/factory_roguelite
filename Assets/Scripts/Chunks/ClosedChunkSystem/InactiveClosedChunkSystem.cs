using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule;

namespace ChunkModule.ClosedChunkSystemModule {
    public class InactiveClosedChunkSystem
    {
        private List<UnloadedConduitTileChunk> unloadedChunks;
        public InactiveClosedChunkSystem(List<UnloadedConduitTileChunk> unloadedChunks) {
            this.unloadedChunks = unloadedChunks;
        }

        public List<UnloadedConduitTileChunk> UnloadedChunks { get => unloadedChunks; set => unloadedChunks = value; }

        public bool chunkIsNeighbor(UnloadedConduitTileChunk unloadedChunk) {
            foreach (UnloadedConduitTileChunk containedChunk in unloadedChunks) {
                Vector2Int dif = containedChunk.getPosition() - unloadedChunk.getPosition();
                if (Mathf.Abs(dif.x) <= 1 && Mathf.Abs(dif.y) <= 1) {
                    return true;
                }
            }
            return false;
        }
        public bool systemIsNeighbor(InactiveClosedChunkSystem inactiveClosedChunkSystem) {
            foreach (UnloadedConduitTileChunk neighborChunk in inactiveClosedChunkSystem.UnloadedChunks) {
                if (chunkIsNeighbor(neighborChunk)) {
                    return true;
                }
            }
            return false;
        }
        public void merge(InactiveClosedChunkSystem inactiveClosedChunkSystem) {
            this.unloadedChunks.AddRange(inactiveClosedChunkSystem.UnloadedChunks);
        }
    }
}

