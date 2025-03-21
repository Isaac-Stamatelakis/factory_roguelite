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
        private IChunkSystem dim0System;
        
        public void FixedUpdate() {
            dim0System?.TickUpdate();
        }

        public void LoadSystems() {
            /*
            string path = WorldLoadUtils.GetDimPath(0);
            List<SoftLoadedConduitTileChunk> unloadedChunks = ChunkIO.GetUnloadedChunks(0,path);
            ClosedChunkSystemAssembler closedChunkSystemAssembler = new ClosedChunkSystemAssembler(unloadedChunks,path,0);
            closedChunkSystemAssembler.LoadSystem();
            dim0System = closedChunkSystemAssembler.ToSoftLoaded();
            Debug.Log("Assembled Dim0System");
            */
        }

        public void SetSoftLoadedSystem(SoftLoadedClosedChunkSystem softLoadedClosedChunkSystem)
        {
            dim0System = softLoadedClosedChunkSystem;
        }
        public ClosedChunkSystem ActivateSystem(PlayerScript playerScript)
        {
            if (dim0System == null) {
                LoadSystems();
            }

            if (dim0System is ClosedChunkSystem closedChunkSystem)
            {
                return closedChunkSystem;
            }
            if (dim0System is SoftLoadedClosedChunkSystem softLoadedClosedChunkSystem)
            {
                GameObject closedChunkSystemObject = new GameObject();
                IntervalVector bounds = WorldCreation.GetDim0Bounds();
                closedChunkSystemObject.name="Dim0System";
                ConduitTileClosedChunkSystem mainArea = closedChunkSystemObject.AddComponent<ConduitTileClosedChunkSystem>();
                string path = WorldLoadUtils.GetDimPath(0);
                List<SoftLoadedConduitTileChunk> unloadedChunks = ChunkIO.GetUnloadedChunks(0,path);
                ClosedChunkSystemAssembler closedChunkSystemAssembler = new ClosedChunkSystemAssembler(unloadedChunks,path,0);
                closedChunkSystemAssembler.LoadSystem(softLoadedClosedChunkSystem.GetSoftLoadableTileEntities());
                //closedChunkSystemAssembler.LoadSystem();
                mainArea.Initialize(
                    this,
                    coveredArea: bounds,
                    dim: 0,
                    closedChunkSystemAssembler,
                    playerScript
                );
                dim0System = mainArea;
                return mainArea;
            }
            throw new System.Exception("Failed to activate dim0 system");
        }
        public void DeactivateSystem()
        {
            if (dim0System is ConduitTileClosedChunkSystem closedChunkSystem)
            {
                dim0System.Save();
                dim0System = closedChunkSystem.ToSoftLoadedSystem();
                GameObject.Destroy(closedChunkSystem.gameObject); 
            }
            
        }
        
        public ClosedChunkSystem GetActiveSystem()
        {
            return dim0System as ClosedChunkSystem;
        }

        public IEnumerator SaveSystemCoroutine()
        {
            yield return dim0System?.SaveCoroutine();
        }

        public void SaveSystem()
        {
            dim0System?.Save();
        }
        
        public IChunkSystem GetSystem()
        {
            return dim0System;
        }
    }
}

