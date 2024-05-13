using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dimensions;
using WorldModule;

namespace DevTools.Structures {
    public class StructureCreatorDimManager : DimensionManager
    {
        [SerializeField] public StructureDimController dimController;
        public override DimController getCurrentController()
        {
            return dimController;
        }

        public override async void Start()
        {
            Debug.Log("Loading Structure Creator: " + WorldManager.getInstance().getWorldPath());
            dimController.getSystem();
            Vector2Int playerCellPosition = Global.getCellPositionFromWorld(playerIO.getPlayerPosition());
            await setActiveSystemFromCellPosition(0,Vector2Int.zero);
            setPlayerPosition(Vector2.zero);
        }
    }
}

