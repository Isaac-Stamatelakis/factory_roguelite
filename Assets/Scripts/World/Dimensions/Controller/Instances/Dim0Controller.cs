using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldModule;
using Chunks.Systems;
using Chunks;
using Chunks.IO;
using Chunks.Partitions;
using Player;

namespace Dimensions {
    public class Dim0Controller : DimController, ISingleSystemController
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

        public void SoftLoadSystem() {
            string path = WorldLoadUtils.GetDimPath(0);
            List<SoftLoadedConduitTileChunk> unloadedChunks = ChunkIO.GetUnloadedChunks(0,path);
            dim0System = new SoftLoadedClosedChunkSystem(unloadedChunks,path);
            dim0System.SoftLoad();
            Debug.Log("Soft loaded Dim0System");
        }
        public ClosedChunkSystem ActivateSystem(PlayerScript playerScript)
        {
            if (dim0System == null) {
                SoftLoadSystem();
            }
            GameObject closedChunkSystemObject = new GameObject();
            IntervalVector bounds = WorldCreation.GetDim0Bounds();
            closedChunkSystemObject.name="Dim0System";
            mainArea = closedChunkSystemObject.AddComponent<ConduitTileClosedChunkSystem>();
            mainArea.Initialize(
                this,
                coveredArea: bounds,
                dim: 0,
                dim0System,
                playerScript
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
        
        public ClosedChunkSystem GetActiveSystem()
        {
            return mainArea;
        }

        public IEnumerator SaveSystemCoroutine()
        {
            yield return dim0System?.SaveCoroutine();
        }

        public void SaveSystem()
        {
            dim0System?.Save();
        }


        public SoftLoadedClosedChunkSystem getSystem()
        {
            return dim0System;
        }
    }
}

