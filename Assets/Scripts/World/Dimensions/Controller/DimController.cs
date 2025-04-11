using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;
using Chunks.Systems;
using Entities;
using Entities.Mob;
using Player;
using Unity.VisualScripting;

namespace Dimensions {

    public interface IMultipleSystemController {
        public ClosedChunkSystem ActivateSystem(IDimensionTeleportKey key, PlayerScript playerScript);
        public ClosedChunkSystem GetActiveSystem();
        public List<IChunkSystem> GetAllSystems();
    }

    public interface ISingleSystemController {
        public ClosedChunkSystem ActivateSystem(PlayerScript playerScript);
        public ClosedChunkSystem GetActiveSystem();
        public IChunkSystem GetSystem();
    }
    
    public abstract class DimController : MonoBehaviour
    {
        public abstract void TickUpdate();
        public abstract ClosedChunkSystem GetActiveSystem();
        public bool BoundCamera = false;
        
        public abstract void DeActivateSystem();
    }
}


