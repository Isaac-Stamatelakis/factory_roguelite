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
using Tiles.TileMap.Interval;

namespace Dimensions {
    public class Dim0Controller : DimController, ISingleSystemController
    {
        private IConduitClosedChunkSystem dim0System;
        private List<SoftLoadedConduitTileChunk> cachedChunks;
        private uint tickCounter;
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
                closedChunkSystemObject.name="Dim0System";
                ConduitClosedChunkSystem mainArea = closedChunkSystemObject.AddComponent<ConduitClosedChunkSystem>();
                string path = WorldLoadUtils.GetDimPath(0);
                ClosedChunkSystemAssembler closedChunkSystemAssembler = new ClosedChunkSystemAssembler(cachedChunks,path,0);
                IntervalVector bounds = closedChunkSystemAssembler.GetBounds();
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
            if (dim0System is not ConduitClosedChunkSystem closedChunkSystem) return;
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
            if (tickCounter % Global.TILE_ENTITY_TICK_RATE == 0)
            {
                dim0System?.TileEntityTickUpdate();
            }

            if (tickCounter % Global.CONDUIT_TICK_RATE == 0)
            {
                dim0System?.ConduitTickUpdate();
            }
            tickCounter++;
        }
    }
}

