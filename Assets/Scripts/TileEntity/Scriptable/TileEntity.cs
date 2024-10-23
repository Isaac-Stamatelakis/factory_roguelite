using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using UnityEngine.Tilemaps;
using Chunks.Partitions;

namespace TileEntityModule {
    public interface ITileEntityInstance {
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
    public abstract class TileEntity : ScriptableObject
    {
        public abstract ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk);
    }
}