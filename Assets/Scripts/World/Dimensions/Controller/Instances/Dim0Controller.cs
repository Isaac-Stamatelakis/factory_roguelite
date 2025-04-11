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
        private List<SoftLoadedConduitTileChunk> cachedChunks;
        public void SetSoftLoadedSystem(SoftLoadedClosedChunkSystem softLoadedClosedChunkSystem, List<SoftLoadedConduitTileChunk> chunks)
        {
            cachedChunks = chunks;
            dim0System = softLoadedClosedChunkSystem;
        }
        public ClosedChunkSystem ActivateSystem(PlayerScript playerScript)
        {
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
                ClosedChunkSystemAssembler closedChunkSystemAssembler = new ClosedChunkSystemAssembler(cachedChunks,path,0);
                closedChunkSystemAssembler.LoadSystem(softLoadedClosedChunkSystem.GetSoftLoadableTileEntities(),false);
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
        public override void DeActivateSystem()
        {
            if (dim0System is not ConduitTileClosedChunkSystem closedChunkSystem) return;
            SoftLoadedClosedChunkSystem softLoadedClosedChunkSystem = closedChunkSystem.ToSoftLoadedSystem();
            dim0System = softLoadedClosedChunkSystem;
            softLoadedClosedChunkSystem.ClearActiveComponents();
            cachedChunks.Clear();
            foreach (ILoadedChunk loadedChunk in closedChunkSystem.CachedChunk.Values)
            {
                SoftLoadedConduitTileChunk softLoadedConduitTileChunk = new SoftLoadedConduitTileChunk(loadedChunk);
                cachedChunks.Add(softLoadedConduitTileChunk);
            }
            GameObject.Destroy(closedChunkSystem.gameObject);
        }
        
        public override ClosedChunkSystem GetActiveSystem()
        {
            return dim0System as ClosedChunkSystem;
        }
        
        public IChunkSystem GetSystem()
        {
            return dim0System;
        }

        public override void TickUpdate()
        {
            dim0System?.TickUpdate();
        }
    }
}

