using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Chunks.Systems;
using Chunks.Partitions;
using Dimensions;

namespace Entities {
    /// <summary>
    /// Singleton monobehavior for which contains all entities
    /// </summary>
    public class EntityContainer : MonoBehaviour
    {
        public void OnDestroy() {
            ClosedChunkSystem system = transform.parent.GetComponentInChildren<ClosedChunkSystem>();
            if (!system) return;
        }
    }

}
