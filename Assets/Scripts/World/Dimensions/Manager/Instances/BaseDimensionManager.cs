using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerModule.IO;
using WorldModule;
using Chunks.ClosedChunkSystemModule;

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
        /*
        public override async void Start() {
            Debug.Log("Loading world: " + WorldLoadUtils.getFullWorldPath());
            Vector2Int playerCellPosition = Global.getCellPositionFromWorld(playerIO.getPlayerPosition());
            setPlayerSystem(playerIO,0,Vector2Int.zero);
            //setActiveSystemFromCellPosition(playerIO.playerData.dim,playerCellPosition);
            //setPlayerPosition(playerIO.getPlayerPosition());
            //await setActiveSystemFromCellPosition(0,Vector2Int.zero);
            //setPlayerPosition(Vector2.zero);
        }
        */

        public override DimController getDimController(int dim) {
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
    }
}

