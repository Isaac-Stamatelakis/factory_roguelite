using System;
using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;
using Chunks.Systems;
using Player;
using TileEntity;
using WorldModule.Caves;

namespace Dimensions {
    public class CaveController : DimController, ISingleSystemController
    {
        private CaveInstance currentCave;
        private TileClosedChunkSystem activeSystem;
        public ClosedChunkSystem ActivateSystem(PlayerScript playerScript)
        {
            if (currentCave == null) return null;
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

        public bool isActive()
        {
            return activeSystem != null;
        }

        public ClosedChunkSystem GetActiveSystem()
        {
            return activeSystem;
        }

        public IChunkSystem GetSystem()
        {
            return activeSystem;
        }

        public override void DeActivateSystem()
        {
            if (!activeSystem) return;
            GameObject.Destroy(activeSystem.gameObject);
            activeSystem = null;
        }

        public override void TickUpdate()
        {
            activeSystem?.TickUpdate();
        }
    }
}


