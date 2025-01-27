using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using UnityEngine.Tilemaps;
using Chunks.Partitions;

namespace TileEntity {
    public interface ITileEntityInstance {
        public TileEntityObject GetTileEntity();
        public Vector2Int getPositionInChunk();
        public Vector2 getWorldPosition();
        public Vector2Int getCellPosition();
        public IChunkPartition getPartition();
        public Vector2Int getPartitionPositionInChunk();
        public Vector2Int getPositionInPartition();
        public TileBase getTile();
        public IChunk getChunk();
        public void setChunk(IChunk chunk);
        public string getName();
    }

    public interface ITieredTileEntity
    {
        public Tier GetTier();
    }
    public abstract class TileEntityObject : ScriptableObject
    {
        [Header("TileEntityInstance will be created with extra partition load range. Useful for torches")]
        public bool ExtraLoadRange;
        public abstract ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk);
    }
}