using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapModule.Type;
using TileMapModule;
using TileMapModule.Layer;
using TileEntityModule;

namespace ChunkModule.PartitionModule {
    public interface IChunkPartition {
        public IChunkPartitionData getData();
        public UnityEngine.Vector2Int getRealPosition();
        public bool getTileLoaded();
        public void setTileLoaded(bool val);
        public bool getEntityLoaded();
        public void setEntityLoaded(bool val);
        public float distanceFrom(UnityEngine.Vector2Int target);
        public bool getScheduledForUnloading();
        public void setScheduleForUnloading(bool val);
        public IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps, double angle);
        public IEnumerator unloadTiles(Dictionary<TileMapType, ITileMap> tileGridMaps);
        public void save(Dictionary<TileMapType, ITileMap> tileGridMaps);
        public bool inRange(Vector2Int target, int xRange, int yRange);
        public void tick();
        public void addTileEntity(TileMapLayer layer,TileEntity tileEntity,Vector2Int positionInPartition);
        public void breakTileEntity(TileMapLayer layer, Vector2Int position);
        public bool clickTileEntity(TileMapLayer layer, Vector2Int position);
    }
}
