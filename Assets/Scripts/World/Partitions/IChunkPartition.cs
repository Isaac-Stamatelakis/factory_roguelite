using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using TileMaps.Type;
using TileMaps;
using TileMaps.Layer;
using TileEntity;
using Tiles;
using Tiles.Fluid.Simulation;

namespace Chunks.Partitions {
    public interface IChunkPartition {
        public SeralizedWorldData GetData();
        public UnityEngine.Vector2Int GetRealPosition();
        public bool GetLoaded();
        public void SetTileLoaded(bool val);
        public float DistanceFrom(UnityEngine.Vector2Int target);
        public bool GetScheduledForUnloading();
        public void SetScheduleForUnloading(bool val);
        public IEnumerator Load(Dictionary<TileMapType, IWorldTileMap> tileGridMaps, Direction direction);
        public IEnumerator UnloadTiles(Dictionary<TileMapType, IWorldTileMap> tileGridMaps);
        public void Save();
        public bool InRange(Vector2Int target, int xRange, int yRange);
        public void Tick();
        public void AddTileEntity(TileMapLayer layer,ITileEntityInstance tileEntity,Vector2Int positionInPartition);
        public void BreakTileEntity(TileMapLayer layer, Vector2Int positionInPartition);
        public ITileEntityInstance GetTileEntity(Vector2Int positionInPartition);
        public List<T> GetTileEntitiesOfType<T>();
        public void GetTileEntityObjects(HashSet<TileEntityObject> objects);
        public TileItem GetTileItem(Vector2Int position,TileMapLayer layer);
        public void SetTile(Vector2Int position, TileMapLayer layer, TileItem tileItem);
        public void AddFluidDataToChunk(FluidCell[][] chunkFluidCells);
        public PartitionFluidData GetFluidData();
        public void UnloadEntities();
        public bool GetFarLoaded();
        public void SetFarLoaded(bool state);
        public void LoadFarLoadTileEntities();
        public void UnloadFarLoadTileEntities();
        public bool GetScheduledForFarLoading();
        public void SetScheduledForFarLoading(bool state);
        public BaseTileData GetBaseData(Vector2Int position);
        public bool DeIncrementHardness(Vector2Int position);
        public int GetHardness(Vector2Int positionInPartition);
        public void UnloadTileEntities();
        public void SetHardness(Vector2Int positionInPartition, int hardness);
        public void SetBaseTileData(Vector2Int positionInPartition, BaseTileData baseTileData);
        public FluidCell GetFluidCell(Vector2Int positionInPartition);
    }
}
