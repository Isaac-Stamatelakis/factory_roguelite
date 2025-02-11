using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.Systems;
using Entities;
using Player;

namespace Dimensions {

    public interface IMultipleSystemController {
        public ClosedChunkSystem ActivateSystem(IDimensionTeleportKey key, PlayerScript playerScript);
        public ClosedChunkSystem GetActiveSystem(IDimensionTeleportKey key);
    }

    public interface ISingleSystemController {
        public ClosedChunkSystem ActivateSystem(PlayerScript playerScript);
        public ClosedChunkSystem GetActiveSystem();
    }
    public abstract class DimController : MonoBehaviour
    {
        private Transform entityContainer;
        public Transform EntityContainer { get => entityContainer;}
        public bool BoundCamera = false;

        public virtual void Awake() {
            GameObject entityContainerObject = new GameObject();
            entityContainerObject.name = "Entities";
            entityContainerObject.transform.SetParent(transform);
            EntityContainer component = entityContainerObject.AddComponent<EntityContainer>();
            entityContainer = entityContainerObject.transform;
        }
    }
}


