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
        public ClosedChunkSystem activateSystem(Vector2Int dimOffsetPosition)
        {
            GameObject closedChunkSystemObject = new GameObject();
            closedChunkSystemObject.name="Cave";
            activeSystem = closedChunkSystemObject.AddComponent<TileClosedChunkSystem>();
            IntervalVector coveredArea = currentCave.getChunkCoveredArea();
            activeSystem.initalize(transform,coveredArea,-1,dimOffsetPosition);
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

        public ClosedChunkSystem getActiveSystem()
        {
            return activeSystem;
        }

    }
}


