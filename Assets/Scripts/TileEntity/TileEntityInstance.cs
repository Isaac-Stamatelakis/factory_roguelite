using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Chunks;
using Chunks.Partitions;

namespace TileEntity {
    public class TileEntityInstance<T> : ITileEntityInstance where T : TileEntityObject
    {
        protected T tileEntityObject;
        public T TileEntityObject => tileEntityObject;
        protected Vector2Int positionInChunk;
        protected TileItem tileItem; 
        protected IChunk chunk;

        public TileEntityInstance(T tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk)
        {
            this.tileEntityObject = tileEntityObject;
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
            return (positionInChunk + chunk.GetPosition() * Global.CHUNK_SIZE);
        }
        public IChunkPartition getPartition() {
            return chunk.GetPartition(getPartitionPositionInChunk());
        }
        public Vector2Int getPartitionPositionInChunk() {
            return Global.getPartitionFromCell(positionInChunk);
        }
        public Vector2Int getPositionInPartition() {
            return positionInChunk-getPartitionPositionInChunk()*Global.CHUNK_PARTITION_SIZE;
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
            return TileEntityObject.name;
        }

        public TileEntityObject GetTileEntity()
        {
            return TileEntityObject;
        }
    }

    public class StandardTileEntityInstance : TileEntityInstance<TileEntityObject>
    {
        public StandardTileEntityInstance(TileEntityObject tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }
    }
}

