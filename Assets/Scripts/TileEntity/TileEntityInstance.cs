using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Chunks;
using Chunks.Partitions;

namespace TileEntityModule {
    public class TileEntityInstance<T> : ITileEntityInstance where T : TileEntity
    {
        protected T tileEntity;
        public T TileEntity => tileEntity;
        protected Vector2Int positionInChunk;
        protected TileItem tileItem; 
        protected IChunk chunk;

        public TileEntityInstance(T tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk)
        {
            this.tileEntity = tileEntity;
            this.positionInChunk = positionInChunk;
            this.tileItem = tileItem;
            this.chunk = chunk;
        }

        public Vector2Int getPositionInChunk() {
            return positionInChunk;
        }

        public Vector2 getWorldPosition() {
            Vector2Int cellPosition = getCellPosition();
            return new Vector2(cellPosition.x/2f+0.25f,cellPosition.y/2f+0.25f);
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
            return tileItem.tile;
        }
        public string getId() {
            return tileItem.id;
        }

        public IChunk getChunk()
        {
            return chunk;
        }

        public void setChunk(IChunk chunk)
        {
            this.chunk = chunk;
        }

        public string getName()
        {
            return tileEntity.name;
        }
    }

    public class StandardTileEntityInstance : TileEntityInstance<TileEntity>
    {
        public StandardTileEntityInstance(TileEntity tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
    }
}

