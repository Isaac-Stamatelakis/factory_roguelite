using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule;
using UnityEngine.Tilemaps;
using ChunkModule.PartitionModule;

namespace TileEntityModule {
    public interface ITileEntity {
        public Vector2Int getPositionInChunk();
        public Vector2 getWorldPosition();
        public Vector2Int getCellPosition();
        public IChunkPartition getPartition();
        public Vector2Int getPartitionPositionInChunk();
        public Vector2Int getPositionInPartition();
        public TileBase getTile();
        public IChunk getChunk();
        public void setChunk(IChunk chunk);
    }
    public abstract class TileEntity : ScriptableObject, ITileEntity
    {
        protected Vector2Int positionInChunk;
        protected IChunk chunk;
        protected TileBase tile;
        public virtual void initalize(Vector2Int tilePosition, TileBase tileBase, IChunk chunk) {
            this.chunk = chunk;
            this.positionInChunk = tilePosition;
            this.tile = tileBase;
        }

        public Vector2Int getPositionInChunk() {
            return positionInChunk;
        }

        public Vector2 getWorldPosition() {
            return (positionInChunk + chunk.getPosition() * Global.ChunkSize)/2;
        }
        public Vector2Int getCellPosition() {
            return (positionInChunk + chunk.getPosition() * Global.ChunkSize);
        }
        public IChunkPartition getPartition() {
            return chunk.getPartition(getPartitionPositionInChunk());
        }
        public Vector2Int getPartitionPositionInChunk() {
            return Global.getPartitionFromCell(positionInChunk);
        }
        public Vector2Int getPositionInPartition() {
            return positionInChunk-getPartitionPositionInChunk()*Global.ChunkPartitionSize;
        }

        public TileBase getTile()
        {
            return tile;
        }

        public IChunk getChunk()
        {
            return chunk;
        }

        public void setChunk(IChunk chunk)
        {
            this.chunk = chunk;
        }
    }
} // end