using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.ClosedChunkSystemModule;
using Entities;

namespace Dimensions {

    public interface IMultipleSystemController {
        public ClosedChunkSystem getSystemFromWorldPosition(Vector2 position);
        public ClosedChunkSystem getSystemFromCellPositon(Vector2Int position);
    }

    public interface ISingleSystemController {
        public ClosedChunkSystem getSystem();
    }
    public abstract class DimController : MonoBehaviour
    {
        private Transform entityContainer;
        public Transform EntityContainer { get => entityContainer;}

        public void Awake() {
            GameObject entityContainerObject = new GameObject();
            entityContainerObject.name = "Entities";
            entityContainerObject.transform.SetParent(transform);
            EntityContainer component = entityContainerObject.AddComponent<EntityContainer>();
            entityContainer = entityContainerObject.transform;
        }
    }
}


