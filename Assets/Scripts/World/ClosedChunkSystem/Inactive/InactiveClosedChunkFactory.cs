using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using WorldModule;
using System;
using Chunks.IO;


namespace Chunks.Systems {
    public static class InactiveClosedChunkFactory 
    {
        public static SoftLoadedClosedChunkSystem import(string path) {
            List<SoftLoadedConduitTileChunk> chunks = ChunkIO.getUnloadedChunks(1,path);
            return new SoftLoadedClosedChunkSystem(chunks,path);
        }
    }

}
