using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.ClosedChunkSystemModule;
using WorldModule.Caves;

namespace Dimensions {
    public class CaveController : DimController, ISingleSystemController
    {
        [SerializeField] public Cave cave;
        private TileClosedChunkSystem activeSystem;
        public ClosedChunkSystem activateSystem(Vector2Int dimOffsetPosition)
        {
            GameObject closedChunkSystemObject = new GameObject();
            closedChunkSystemObject.name="Cave";
            activeSystem = closedChunkSystemObject.AddComponent<TileClosedChunkSystem>();
            IntervalVector coveredArea = cave.getChunkCoveredArea();
            activeSystem.initalize(transform,coveredArea,-1,dimOffsetPosition);
            return activeSystem;
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


