#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dimensions;
using Chunks.ClosedChunkSystemModule;
using Chunks;
using WorldModule;
using Chunks.IO;
using System.IO;



namespace DevTools.Structures {
    public class StructureDimController : DimController, ISingleSystemController
    {
        private SoftLoadedClosedChunkSystem system;
        public ClosedChunkSystem getSystem()
        {
            if (system == null) {
                string path = WorldLoadUtils.getDimPath(0);
                List<SoftLoadedConduitTileChunk> unloadedChunks = ChunkIO.getUnloadedChunks(0,path);
                system = new SoftLoadedClosedChunkSystem(unloadedChunks);
                system.softLoad();
            }
            GameObject closedChunkSystemObject = new GameObject();
            IntervalVector bounds = WorldCreation.getDim0Bounds();
            closedChunkSystemObject.name="Structure";
            ConduitTileClosedChunkSystem mainArea = closedChunkSystemObject.AddComponent<ConduitTileClosedChunkSystem>();
            mainArea.initalize(
                transform,
                coveredArea: bounds,
                dim: 0,
                system
            );
            return mainArea;
        }

        public void OnDestroy() {
            system.save();
        }
    }
}

#endif