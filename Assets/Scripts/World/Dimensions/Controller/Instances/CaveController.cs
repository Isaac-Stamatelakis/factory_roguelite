using System;
using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;
using Chunks.Systems;
using Player;
using TileEntity;
using Tiles.TileMap.Interval;
using UI.Indicators;
using WorldModule.Caves;

namespace Dimensions {
    public class SerializedCaveDimData
    {
        public string CaveId;
        public float X;
        public float Y;
    }
    public class CaveController : DimController, ISingleSystemController
    {
        private string caveId;
        private IntervalVector coveredArea;
        private TileClosedChunkSystem activeSystem;
        
        private Vector2 returnPortalLocation;
        public Vector2 ReturnPortalLocation => returnPortalLocation;

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
        public void setCurrentCave(CaveObject caveObject, Vector2 returnPortalLocation) {
            caveId = caveObject.GetId();
            coveredArea = caveObject.GetChunkCoveredArea();
            this.returnPortalLocation = returnPortalLocation;
        }
        
        public override ClosedChunkSystem GetActiveSystem()
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

        
        public SerializedCaveDimData GetSerializedCaveDimData()
        {
            return new SerializedCaveDimData
            {
                CaveId = caveId,
                X = returnPortalLocation.x,
                Y = returnPortalLocation.y
            };
        }
    }
}


