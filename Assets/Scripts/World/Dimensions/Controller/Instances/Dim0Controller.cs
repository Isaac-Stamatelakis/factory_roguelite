using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldModule;
using Chunks.ClosedChunkSystemModule;
using Chunks;
using Chunks.IO;
using Chunks.Partitions;

namespace Dimensions {
    public class Dim0Controller : DimController, ISingleSystemController
    {
        private SoftLoadedClosedChunkSystem dim0System;
        public void FixedUpdate() {
            dim0System.tickUpdate();
        }
        public ClosedChunkSystem getSystem()
        {
            if (dim0System == null) {
                string path = WorldLoadUtils.getDimPath(0);
                List<SoftLoadedConduitTileChunk> unloadedChunks = ChunkIO.getUnloadedChunks(0,path);
                dim0System = new SoftLoadedClosedChunkSystem(unloadedChunks);
                dim0System.softLoad();
            }
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

