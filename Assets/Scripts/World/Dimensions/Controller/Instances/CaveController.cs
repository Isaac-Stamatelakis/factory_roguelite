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
        private string caveId;
        private IntervalVector coveredArea;
        private TileClosedChunkSystem activeSystem;

        public string GetCurrentCaveId()
        {
            return caveId;
        }
        public ClosedChunkSystem ActivateSystem(PlayerScript playerScript)
        {
            if (caveId == null) return null;
            GameObject closedChunkSystemObject = new GameObject();
            closedChunkSystemObject.name="Cave";
            activeSystem = closedChunkSystemObject.AddComponent<TileClosedChunkSystem>();
            
            activeSystem.Initalize(this,coveredArea,-1);
            return activeSystem;
        }
        public void setCurrentCave(CaveObject caveObject) {
            caveId = caveObject.GetId();
            coveredArea = caveObject.GetChunkCoveredArea();
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


