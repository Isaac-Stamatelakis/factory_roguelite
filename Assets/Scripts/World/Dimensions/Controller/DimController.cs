using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;
using Chunks.Systems;
using Entities;
using Player;

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
        private Transform entityContainer;
        public abstract void TickUpdate();
        public Transform EntityContainer { get => entityContainer;}
        public bool BoundCamera = false;

        public virtual void Awake() {
            GameObject entityContainerObject = new GameObject();
            entityContainerObject.name = "Entities";
            entityContainerObject.transform.SetParent(transform);
            EntityContainer component = entityContainerObject.AddComponent<EntityContainer>();
            entityContainer = entityContainerObject.transform;
        }

        public void ClearEntities()
        {
            GlobalHelper.deleteAllChildren(entityContainer);
        }
        
        public abstract void DeActivateSystem();
    }
}


