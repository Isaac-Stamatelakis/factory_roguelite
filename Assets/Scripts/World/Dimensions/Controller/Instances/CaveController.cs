using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.Systems;
using WorldModule.Caves;

namespace Dimensions {
    public class CaveController : DimController, ISingleSystemController
    {
        private CaveInstance currentCave;
        private TileClosedChunkSystem activeSystem;
        public ClosedChunkSystem ActivateSystem()
        {
            GameObject closedChunkSystemObject = new GameObject();
            closedChunkSystemObject.name="Cave";
            activeSystem = closedChunkSystemObject.AddComponent<TileClosedChunkSystem>();
            IntervalVector coveredArea = currentCave.getChunkCoveredArea();
            activeSystem.Initalize(this,coveredArea,-1);
            return activeSystem;
        }
        public void setCurrentCave(CaveInstance caveInstance) {
            this.currentCave = caveInstance;
        }
        public void deactivateSystem()
        {
            GameObject.Destroy(activeSystem.gameObject);
        }

        public bool isActive()
        {
            return activeSystem != null;
        }

        public ClosedChunkSystem GetActiveSystem()
        {
            return activeSystem;
        }

    }
}


