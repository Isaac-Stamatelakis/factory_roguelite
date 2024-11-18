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
        public static SoftLoadedClosedChunkSystem Import(string path) {
            var chunks = ChunkIO.getUnloadedChunks(1,path);
            return new SoftLoadedClosedChunkSystem(chunks,path);
        }
    }

}
