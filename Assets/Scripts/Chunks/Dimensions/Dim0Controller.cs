using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldModule;
using ChunkModule.ClosedChunkSystemModule;
using ChunkModule;
using ChunkModule.IO;

namespace DimensionModule {
    public class Dim0Controller : DimController
    {
        public override void Start() {
            base.Start();
            GameObject closedChunkSystemObject = new GameObject();
            closedChunkSystemObject.name="Dim0System";
            ConduitTileClosedChunkSystem mainArea = closedChunkSystemObject.AddComponent<ConduitTileClosedChunkSystem>();
            activeSystem = mainArea;
            IntervalVector bounds = WorldCreation.getDim0Bounds();
            List<SoftLoadedConduitTileChunk> unloadedChunks = ChunkIO.getUnloadedChunks(0);
            SoftLoadedClosedChunkSystem inactiveClosedChunkSystem = new SoftLoadedClosedChunkSystem(unloadedChunks);
            inactiveClosedChunkSystem.softLoad();
            mainArea.initalize(
                transform,
                coveredArea: bounds,
                dim: 0,
                inactiveClosedChunkSystem
            );
        }
    }
}

