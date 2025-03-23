using System;
using System.Collections;
using System.Collections.Generic;
using Chunks;
using Chunks.IO;
using UnityEngine;
using PlayerModule.IO;
using WorldModule;
using Chunks.Systems;
using Player;
using World.Serialization;

namespace Dimensions {
    public enum Dimension {
        OverWorld = 0,
        Cave = -1,
        CompactMachine = 1,
    }
    public class BaseDimensionManager : DimensionManager, ICompactMachineDimManager
    {
        [SerializeField] public Dim0Controller overworldDimController;
        [SerializeField] public CaveController caveDimController;
        [SerializeField] public CompactMachineDimController compactMachineDimController;

        
        public override DimController GetDimController(Dimension dimension) {
            switch (dimension) {
                case Dimension.OverWorld:
                    return overworldDimController;
                case Dimension.Cave:
                    return caveDimController;
                case Dimension.CompactMachine:
                    return compactMachineDimController;
            }
            return null;
        }

        public CompactMachineDimController GetCompactMachineDimController() {
            return compactMachineDimController;
        }
        

        protected override List<DimController> GetAllControllers()
        {
            return new List<DimController>
            {
                overworldDimController,
                caveDimController,
                compactMachineDimController
            };
        }

        protected override void TickUpdate()
        {
            if (activeSystem.Dim == (int)Dimension.Cave)
            {
                caveDimController.TickUpdate();
            }
            overworldDimController.TickUpdate();
            compactMachineDimController.TickUpdate();
        }

        public override void SoftLoadSystems()
        {
            string path = WorldLoadUtils.GetDimPath(0);
            List<SoftLoadedConduitTileChunk> unloadedChunks = ChunkIO.GetUnloadedChunks(0,path);
            ClosedChunkSystemAssembler dim0SystemAssembler = new ClosedChunkSystemAssembler(unloadedChunks,path,0);
            dim0SystemAssembler.LoadSystem();
            SoftLoadedClosedChunkSystem softLoadedClosedChunkSystem = dim0SystemAssembler.ToSoftLoaded();
            overworldDimController.SetSoftLoadedSystem(softLoadedClosedChunkSystem,unloadedChunks);
            compactMachineDimController.softLoadSystem(dim0SystemAssembler,overworldDimController);
        }
        
    }
}

