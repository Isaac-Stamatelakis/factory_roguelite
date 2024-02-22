using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule;
using UnityEngine.Tilemaps;
using ChunkModule.PartitionModule;

namespace TileEntityModule {
    public abstract class TileEntity : ScriptableObject
    {
        protected Vector2Int tilePosition;
        protected IChunk chunk;
        protected TileBase tile;
        public virtual void initalize(Vector2Int tilePosition, TileBase tileBase, IChunk chunk) {
            this.chunk = chunk;
            this.tilePosition = tilePosition;
            this.tile = tileBase;
        }

        public Vector2Int getPositionInChunk() {
            return tilePosition;
        }

        public Vector2 getWorldPosition() {
            return (tilePosition + chunk.getPosition() * Global.ChunkSize)/2;
        }
        public Vector2Int getCellPosition() {
            return (tilePosition + chunk.getPosition() * Global.ChunkSize);
        }
        public IChunkPartition getPartition() {
            return chunk.getPartition(getPartitionPositionInChunk());
        }
        public Vector2Int getPartitionPositionInChunk() {
            return Global.getPartitionFromCell(tilePosition);
        }
        public Vector2Int getPositionInPartition() {
            return tilePosition-getPartitionPositionInChunk()*Global.ChunkPartitionSize;
        }
    }
} // end