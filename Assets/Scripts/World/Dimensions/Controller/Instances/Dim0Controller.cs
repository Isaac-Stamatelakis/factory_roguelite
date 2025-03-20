using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldModule;
using Chunks.Systems;
using Chunks;
using Chunks.IO;
using Chunks.Partitions;
using Player;
using TileEntity;

namespace Dimensions {
    public class Dim0Controller : DimController, ISingleSystemController
    {
        private LoadedClosedChunkSystem dim0System;
        private ConduitTileClosedChunkSystem mainArea;
        
        public void FixedUpdate() {
            dim0System?.TickUpdate();
        }

        public void LoadSystems() {
            string path = WorldLoadUtils.GetDimPath(0);
            List<SoftLoadedConduitTileChunk> unloadedChunks = ChunkIO.GetUnloadedChunks(0,path);
            dim0System = new LoadedClosedChunkSystem(unloadedChunks,path,0);
            dim0System.LoadSystem();
            SoftLoadedClosedChunkSystem softLoaded = dim0System.ToSoftLoaded();
            Debug.Log(softLoaded);
            Debug.Log("Soft loaded Dim0System");
        }
        public ClosedChunkSystem ActivateSystem(PlayerScript playerScript)
        {
            if (dim0System == null) {
                LoadSystems();
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

        public LoadedClosedChunkSystem GetInactiveSystem()
        {
            return dim0System;
        }


        public LoadedClosedChunkSystem getSystem()
        {
            return dim0System;
        }
    }
}

