using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldModule;
using Chunks.Systems;
using Chunks;
using Chunks.IO;
using Chunks.Partitions;

namespace Dimensions {
    public interface ISoftLoadableDimension {
        public void softLoadSystem();
        public SoftLoadedClosedChunkSystem getSystem();
    }
    public class Dim0Controller : DimController, ISingleSystemController, ISoftLoadableDimension
    {
        private SoftLoadedClosedChunkSystem dim0System;
        private ConduitTileClosedChunkSystem mainArea;
        public override void Awake() {
            base.Awake();
        }
        public void FixedUpdate() {
            if (dim0System == null) {
                return;
            }
            dim0System.TickUpdate();
        }

        public void softLoadSystem() {
            string path = WorldLoadUtils.getDimPath(0);
            List<SoftLoadedConduitTileChunk> unloadedChunks = ChunkIO.getUnloadedChunks(0,path);
            dim0System = new SoftLoadedClosedChunkSystem(unloadedChunks,path);
            dim0System.softLoad();
            Debug.Log("Soft loaded Dim0System");
        }
        public ClosedChunkSystem activateSystem(Vector2Int dimOffset)
        {
            if (dim0System == null) {
                softLoadSystem();
            }
            GameObject closedChunkSystemObject = new GameObject();
            IntervalVector bounds = WorldCreation.GetDim0Bounds();
            closedChunkSystemObject.name="Dim0System";
            mainArea = closedChunkSystemObject.AddComponent<ConduitTileClosedChunkSystem>();
            mainArea.initalize(
                this,
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
            dim0System.Save();
        }
        public ClosedChunkSystem getActiveSystem()
        {
            return mainArea;
        }

        public SoftLoadedClosedChunkSystem getSystem()
        {
            return dim0System;
        }
    }
}

