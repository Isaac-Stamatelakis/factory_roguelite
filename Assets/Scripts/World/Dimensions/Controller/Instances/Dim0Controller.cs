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
        private ConduitTileClosedChunkSystem mainArea;
        public void FixedUpdate() {
            dim0System.tickUpdate();
        }
        public ClosedChunkSystem activateSystem(Vector2Int dimOffset)
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
            mainArea = closedChunkSystemObject.AddComponent<ConduitTileClosedChunkSystem>();
            mainArea.initalize(
                transform,
                coveredArea: bounds,
                dim: 0,
                dim0System,
                dimOffset
            );
            return mainArea;
        }
        public void deactivateSystem()
        {
            GameObject.Destroy(mainArea.gameObject);
        }

        public bool isActive()
        {
            return mainArea != null;
        }

        public void OnDestroy() {
            dim0System.save();
        }
        public ClosedChunkSystem getActiveSystem()
        {
            return mainArea;
        }
    }
}

