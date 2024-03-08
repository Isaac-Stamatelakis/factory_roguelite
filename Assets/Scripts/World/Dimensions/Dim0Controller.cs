using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldModule;
using ChunkModule.ClosedChunkSystemModule;
using ChunkModule;
using ChunkModule.IO;
using ChunkModule.PartitionModule;

namespace DimensionModule {
    public class Dim0Controller : DimController
    {
        private SoftLoadedClosedChunkSystem dim0System;
        public void FixedUpdate() {
            foreach (SoftLoadedConduitTileChunk chunk in dim0System.UnloadedChunks) {
                foreach (IChunkPartition partition in chunk.Partitions) {
                    partition.tick();
                }
            }
        }

        public void Start() {
            List<SoftLoadedConduitTileChunk> unloadedChunks = ChunkIO.getUnloadedChunks(0);
            dim0System = new SoftLoadedClosedChunkSystem(unloadedChunks);
            dim0System.softLoad();
        }
        public override ClosedChunkSystem getSystem(Vector2 position)
        {
            GameObject closedChunkSystemObject = new GameObject();
            IntervalVector bounds = WorldCreation.getDim0Bounds();
            closedChunkSystemObject.name="Dim0System";
            ConduitTileClosedChunkSystem mainArea = closedChunkSystemObject.AddComponent<ConduitTileClosedChunkSystem>();
            mainArea.initalize(
                transform,
                coveredArea: bounds,
                dim: 0,
                dim0System
            );
            return mainArea;
        }
    }
}

