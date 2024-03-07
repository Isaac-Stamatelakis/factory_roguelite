using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule;
using ChunkModule.IO;
using ChunkModule.ClosedChunkSystemModule;

namespace DimensionModule {
    public class CompactMachineDimController : DimController
    {
        List<InactiveClosedChunkSystem> inactiveClosedChunkSystems;
        public override void Start()
        {
            base.Start();
            inactiveClosedChunkSystems =  new List<InactiveClosedChunkSystem>();
            List<UnloadedConduitTileChunk> unloadedChunks = ChunkIO.getUnloadedChunks(1);
            Debug.Log(name +  " Loaded " +  unloadedChunks.Count + " Chunks");
            formSystems(unloadedChunks);
            Debug.Log(name + " Loaded " + inactiveClosedChunkSystems.Count + " Closed Chunk Systems");
        }

        private void formSystems(List<UnloadedConduitTileChunk> unloadedChunks) {
            foreach (UnloadedConduitTileChunk unloadedChunk in unloadedChunks) {
                bool found = false;
                for (int i = inactiveClosedChunkSystems.Count-1; i >= 0; i--) {
                    InactiveClosedChunkSystem inactiveClosedChunkSystem = inactiveClosedChunkSystems[i];
                    found = inactiveClosedChunkSystem.chunkIsNeighbor(unloadedChunk);
                    if (found) {
                        inactiveClosedChunkSystem.UnloadedChunks.Add(unloadedChunk);
                        for (int j = inactiveClosedChunkSystems.Count-1; j >= 0; j--) {
                            if (i == j) {
                                continue;
                            }
                            if (!inactiveClosedChunkSystem.systemIsNeighbor(inactiveClosedChunkSystems[j])) {
                                continue;
                            }
                            inactiveClosedChunkSystem.merge(inactiveClosedChunkSystems[j]);
                            inactiveClosedChunkSystems.RemoveAt(j);
                            break;
                        }
                        break;
                    }
                }
                if (!found) {
                    inactiveClosedChunkSystems.Add(new InactiveClosedChunkSystem(new List<UnloadedConduitTileChunk>{unloadedChunk}));
                }
            }

        }
    }
}

