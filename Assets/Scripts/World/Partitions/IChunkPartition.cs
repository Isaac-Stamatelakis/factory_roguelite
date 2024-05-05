using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapModule.Type;
using TileMapModule;
using TileMapModule.Layer;
using TileEntityModule;
using Tiles;

namespace ChunkModule.PartitionModule {
    public interface IChunkPartition {
        public IChunkPartitionData getData();
        public UnityEngine.Vector2Int getRealPosition();
        public bool getLoaded();
        public void setTileLoaded(bool val);
        public float distanceFrom(UnityEngine.Vector2Int target);
        public bool getScheduledForUnloading();
        public void setScheduleForUnloading(bool val);
        public IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps, double angle);
        public IEnumerator unloadTiles(Dictionary<TileMapType, ITileMap> tileGridMaps);
        public void save();
        public bool inRange(Vector2Int target, int xRange, int yRange);
        public void tick();
        public void addTileEntity(TileMapLayer layer,TileEntity tileEntity,Vector2Int positionInPartition);
        public void breakTileEntity(TileMapLayer layer, Vector2Int position);
        public bool clickTileEntity(Vector2Int position);
        public TileOptions getTileOptions(Vector2Int position);
        public TileEntity GetTileEntity(Vector2Int position);
        public TileItem GetTileItem(Vector2Int position,TileMapLayer layer);
        public void setTile(Vector2Int position, TileMapLayer layer, TileItem tileItem);
        public (string[,],string[,], int[,]) getFluidData();
    }
}
