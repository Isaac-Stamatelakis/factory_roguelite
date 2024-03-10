using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldModule;
using ChunkModule.ClosedChunkSystemModule;
using ChunkModule;
using ChunkModule.IO;
using ChunkModule.PartitionModule;

namespace DimensionModule {
    public class Dim0Controller : DimController, ISingleSystemController
    {
        private SoftLoadedClosedChunkSystem dim0System;
        public void FixedUpdate() {
            dim0System.tickUpdate();
        }

        public void Start() {
            List<SoftLoadedConduitTileChunk> unloadedChunks = ChunkIO.getUnloadedChunks(0);
            dim0System = new SoftLoadedClosedChunkSystem(unloadedChunks);
            dim0System.softLoad();
        }

        public ClosedChunkSystem getSystem()
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

        public void OnDestroy() {
            dim0System.save();
        }
    }
}

