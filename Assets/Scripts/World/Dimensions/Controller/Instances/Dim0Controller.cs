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
            if (ReferenceEquals(dim0System,null)) {
                return;
            }
            dim0System.TickUpdate();
        }

        public void softLoadSystem() {
            string path = WorldLoadUtils.GetDimPath(0);
            List<SoftLoadedConduitTileChunk> unloadedChunks = ChunkIO.GetUnloadedChunks(0,path);
            dim0System = new SoftLoadedClosedChunkSystem(unloadedChunks,path);
            dim0System.softLoad();
            Debug.Log("Soft loaded Dim0System");
        }
        public ClosedChunkSystem ActivateSystem()
        {
            if (dim0System == null) {
                softLoadSystem();
            }
            GameObject closedChunkSystemObject = new GameObject();
            IntervalVector bounds = WorldCreation.GetDim0Bounds();
            closedChunkSystemObject.name="Dim0System";
            mainArea = closedChunkSystemObject.AddComponent<ConduitTileClosedChunkSystem>();
            mainArea.Initialize(
                this,
                coveredArea: bounds,
                dim: 0,
                dim0System
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
        public ClosedChunkSystem GetActiveSystem()
        {
            return mainArea;
        }

        public SoftLoadedClosedChunkSystem getSystem()
        {
            return dim0System;
        }
    }
}

