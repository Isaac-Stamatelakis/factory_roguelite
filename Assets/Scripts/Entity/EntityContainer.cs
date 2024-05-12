using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule;
using ChunkModule.ClosedChunkSystemModule;
using ChunkModule.PartitionModule;
using Dimensions;

namespace Entities {
    /// <summary>
    /// Singleton monobehavior for which contains all entities
    /// </summary>
    public class EntityContainer : MonoBehaviour
    {
        public void OnDisable() {
            ClosedChunkSystem system = transform.parent.GetComponentInChildren<ClosedChunkSystem>();

            DimController dimController = transform.parent.GetComponent<DimController>();
            for (int i = 0; i < transform.childCount; i++) {
                Transform entityTransform = transform.GetChild(i);
                Entity entity = entityTransform.GetComponent<Entity>();
                if (entity == null || entity is not ISerializableEntity serializableEntity) {
                    continue;
                }
                SeralizedEntityData seralizedEntityData = serializableEntity.serialize();
                if (seralizedEntityData == null) {
                    continue;
                }
                Vector2 position = new Vector2(seralizedEntityData.x,seralizedEntityData.y);
                Vector2Int chunkPosition = Global.getChunkFromWorld(position);
                IChunk chunk = system.getChunk(chunkPosition);
                if (chunk == null) {
                    continue;
                }
                Vector2Int partitionPosition = Global.getPartitionFromWorld(position) - chunkPosition*Global.PartitionsPerChunk;

                IChunkPartition partition = chunk.getPartition(partitionPosition);
                IChunkPartitionData partitionData = partition.getData();
                if (partitionData is not SerializedTileData serializedTileData) {
                    continue;
                }
                serializedTileData.entityData.Add(seralizedEntityData);
            }
        }
    }

}
