using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using UnityEngine.Tilemaps;
using Chunks.Partitions;

namespace TileEntity {
    public enum TileEntityUIMode
    {
        Standard,
        Locked
    }
    public interface ITileEntityInstance {
        public TileEntityObject GetTileEntity();
        public Vector2Int GetPositionInChunk();
        public Vector2 GetWorldPosition();
        public Vector2Int GetCellPosition();
        public IChunkPartition GetPartition();
        public Vector2Int GetPartitionPositionInChunk();
        public Vector2Int GetPositionInPartition();
        public TileBase GetTile();
        public IChunk GetChunk();
        public void SetChunk(IChunk chunk);
        public string GetName();
        public TileEntityUIMode GetUIMode();
    }

    public interface ITieredTileEntity
    {
        public Tier GetTier();
    }
    public abstract class TileEntityObject : ScriptableObject
    {
        public abstract ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk);
    }
}