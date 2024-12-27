using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMaps.Type;
using TileMaps;
using TileMaps.Layer;
using TileEntity;
using Tiles;

namespace Chunks.Partitions {
    public interface IChunkPartition {
        public SeralizedWorldData getData();
        public UnityEngine.Vector2Int getRealPosition();
        public bool getLoaded();
        public void setTileLoaded(bool val);
        public float distanceFrom(UnityEngine.Vector2Int target);
        public bool getScheduledForUnloading();
        public void setScheduleForUnloading(bool val);
        public IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps, Direction direction, Vector2Int systemOffset);
        public IEnumerator unloadTiles(Dictionary<TileMapType, ITileMap> tileGridMaps);
        public void save();
        public bool inRange(Vector2Int target, int xRange, int yRange);
        public void tick();
        public void addTileEntity(TileMapLayer layer,ITileEntityInstance tileEntity,Vector2Int positionInPartition);
        public void breakTileEntity(TileMapLayer layer, Vector2Int position);
        public bool clickTileEntity(Vector2Int position);
        public TileOptions getTileOptions(Vector2Int position);
        public ITileEntityInstance GetTileEntity(Vector2Int position);
        public TileItem GetTileItem(Vector2Int position,TileMapLayer layer);
        public void setTile(Vector2Int position, TileMapLayer layer, TileItem tileItem);
        public PartitionFluidData getFluidData();
        public void unloadEntities();
        public bool getFarLoaded();
        public void setFarLoaded(bool state);
        public void loadFarLoadTileEntities();
        public void unloadFarLoadTileEntities();
        public bool getScheduledForFarLoading();
        public void setScheduledForFarLoading(bool state);
    }
}
