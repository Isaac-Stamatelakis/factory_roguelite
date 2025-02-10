#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dimensions;
using Chunks.Systems;
using Chunks;
using WorldModule;
using Chunks.IO;
using System.IO;



namespace DevTools.Structures {
    public class StructureDimController : DimController, ISingleSystemController
    {
        private SoftLoadedClosedChunkSystem system;
        private ConduitTileClosedChunkSystem activeSystem;

        public void deactivateSystem()
        {
            GameObject.Destroy(activeSystem.gameObject);
        }

        public ClosedChunkSystem ActivateSystem()
        {
            if (system == null) {
                string path = WorldLoadUtils.GetDimPath(0);
                List<SoftLoadedConduitTileChunk> unloadedChunks = ChunkIO.GetUnloadedChunks(0,path);
                system = new SoftLoadedClosedChunkSystem(unloadedChunks,path);
                system.SoftLoad();
            }
            GameObject closedChunkSystemObject = new GameObject();
            IntervalVector bounds = WorldCreation.GetDim0Bounds();
            closedChunkSystemObject.name="Structure";
            activeSystem = closedChunkSystemObject.AddComponent<ConduitTileClosedChunkSystem>();
            activeSystem.Initialize(
                this,
                coveredArea: bounds,
                dim: 0,
                system
            );
            return activeSystem;
        }

        public bool isActive()
        {
            return system != null;
        }

        public void OnDestroy() {
            system.Save();
        }

        public ClosedChunkSystem GetActiveSystem()
        {
            return activeSystem;
        }
    }
}

#endif