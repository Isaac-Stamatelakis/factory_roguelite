using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.Systems;
using Entities;

namespace Dimensions {

    public interface IMultipleSystemController {
        public ClosedChunkSystem activateSystem(IDimensionTeleportKey key,Vector2Int dimOffsetPosition);
        public ClosedChunkSystem getActiveSystem(IDimensionTeleportKey key);
    }

    public interface ISingleSystemController {
        public ClosedChunkSystem activateSystem(Vector2Int dimOffsetPosition);
        public ClosedChunkSystem getActiveSystem();
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


