using System;
using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;
using Chunks.Systems;
using Player;
using TileEntity;
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
        private CaveIndicatorUI caveIndicatorUI;
        private Vector2 returnPortalLocation;
        private PlayerScript playerScript;
        

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
            this.playerScript = playerScript;
            activeSystem.Initalize(this,coveredArea,-1);
            caveIndicatorUI = playerScript.PlayerUIContainer.MiscStatusManager.CaveIndicatorUI;
            return activeSystem;
        }
        public void setCurrentCave(CaveObject caveObject, Vector2 returnPortalLocation) {
            caveId = caveObject.GetId();
            coveredArea = caveObject.GetChunkCoveredArea();
            this.returnPortalLocation = returnPortalLocation;
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

        public void Update()
        {
            if (!playerScript) return;
            Vector2 dif = returnPortalLocation-(Vector2)playerScript.transform.position;
            caveIndicatorUI?.UpdateRotation(dif.normalized,dif.magnitude);
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


