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

        public ClosedChunkSystem activateSystem(Vector2Int dimPositionOffset)
        {
            if (system == null) {
                string path = WorldLoadUtils.GetDimPath(0);
                List<SoftLoadedConduitTileChunk> unloadedChunks = ChunkIO.getUnloadedChunks(0,path);
                system = new SoftLoadedClosedChunkSystem(unloadedChunks,path);
                system.softLoad();
            }
            GameObject closedChunkSystemObject = new GameObject();
            IntervalVector bounds = WorldCreation.GetDim0Bounds();
            closedChunkSystemObject.name="Structure";
            activeSystem = closedChunkSystemObject.AddComponent<ConduitTileClosedChunkSystem>();
            activeSystem.initalize(
                this,
                coveredArea: bounds,
                dim: 0,
                system,
                dimPositionOffset
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

        public ClosedChunkSystem getActiveSystem()
        {
            return activeSystem;
        }
    }
}

#endif