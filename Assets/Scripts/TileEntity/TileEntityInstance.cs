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
        protected Vector2Int chunkPosition;
        protected TileItem tileItem; 
        protected IChunk chunk;

        public TileEntityInstance(T tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk)
        {
            this.tileEntityObject = tileEntityObject;
            this.positionInChunk = positionInChunk;
            this.tileItem = tileItem;
            this.chunk = chunk;
            this.chunkPosition = this.chunk.GetPosition();
        }

        public Vector2Int GetPositionInChunk() {
            return positionInChunk;
        }

        public Vector2 GetWorldPosition() {
            Vector2Int cellPosition = GetCellPosition();
            return new Vector2(cellPosition.x/2f+0.25f,cellPosition.y/2f+0.25f);
        }
        public Vector2Int GetCellPosition() {
            return (positionInChunk + chunkPosition * Global.CHUNK_SIZE);
        }
        public IChunkPartition GetPartition() {
            return chunk.GetPartition(GetPartitionPositionInChunk());
        }
        public Vector2Int GetPartitionPositionInChunk() {
            return Global.getPartitionFromCell(positionInChunk);
        }
        public Vector2Int GetPositionInPartition() {
            return positionInChunk-GetPartitionPositionInChunk()*Global.CHUNK_PARTITION_SIZE;
        }

        public TileBase GetTile()
        {
            return tileItem.tile;
        }
        public string getId() {
            return tileItem.id;
        }

        public IChunk GetChunk()
        {
            return chunk;
        }

        public void SetChunk(IChunk chunk)
        {
            if (chunk != null)
            {
                this.chunkPosition = chunk.GetPosition();
            }
            this.chunk = chunk;
        }

        public string GetName()
        {
            return TileEntityObject.name;
        }

        public TileEntityUIMode GetUIMode()
        {
            if (chunk is not ILoadedChunk loadedChunk) return TileEntityUIMode.Standard;
            return loadedChunk.GetSystem().Interactable ? TileEntityUIMode.Standard : TileEntityUIMode.Locked;
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

