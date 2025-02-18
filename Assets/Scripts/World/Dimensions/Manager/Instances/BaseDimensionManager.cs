using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerModule.IO;
using WorldModule;
using Chunks.Systems;
using World.Serialization;

namespace Dimensions {
    public enum Dimension {
        OverWorld,
        Cave,
        CompactMachine
    }
    public class BaseDimensionManager : DimensionManager, ICompactMachineDimManager
    {
        [SerializeField] public Dim0Controller overworldDimController;
        [SerializeField] public CaveController caveDimController;
        [SerializeField] public CompactMachineDimController compactMachineDimController;
        
        public override DimController GetDimController(int dim) {
            switch (dim) {
                case 0:
                    return overworldDimController;
                case -1:
                    return caveDimController;
                case 1:
                    return compactMachineDimController;
            }
            return null;
        }

        public Dim0Controller getDim0Controller() {
            return overworldDimController;
        }

        public CaveController getCaveController() {
            return caveDimController;
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

        public override void SoftLoadSystems()
        {
            overworldDimController.SoftLoadSystem();
            compactMachineDimController.softLoadSystem(overworldDimController.getSystem(),overworldDimController);
        }
        
    }
}

