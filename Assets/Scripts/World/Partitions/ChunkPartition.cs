using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity;
using TileMaps;
using TileMaps.Layer;
using TileMaps.Type;
using Tiles;
using Items;
using Conduits.Ports;
using Entities;

namespace Chunks.Partitions {
    
    public abstract class ChunkPartition<T> : IChunkPartition where T : SeralizedWorldData
    {
        protected bool loaded;
        protected bool farLoaded;
        protected bool scheduledForUnloading = false;
        protected bool scheduledForFarLoading = false;
        protected Vector2Int position;
        protected T data;
        public ITileEntityInstance[,] tileEntities;

        protected int[,] baseTileHardnessArray;
        //public TileOptions[,] tileOptionsArray;
        public List<ITickableTileEntity> tickableTileEntities;
        protected IChunk parent;

        public ChunkPartition(T data, Vector2Int position, IChunk parent) {
            this.data = data;
            this.position = position;
            this.parent = parent;
        }

        public float DistanceFrom(Vector2Int target)
        {
            Vector2Int realPosition = GetRealPosition();
            return Mathf.Pow(target.x-realPosition.x,2) + Mathf.Pow(target.y-realPosition.y,2);
        }

        public SeralizedWorldData GetData()
        {
            return data;
        }


        public void Tick() {
            if (tickableTileEntities == null) {
                return;
            }
            
            foreach (ITickableTileEntity tileEntity in tickableTileEntities) {
                tileEntity.tickUpdate();
            }
        }

        public UnityEngine.Vector2Int GetRealPosition()
        {
            return parent.getPosition()*Global.PartitionsPerChunk + position;
        }

        public bool GetScheduledForUnloading()
        {
            return scheduledForUnloading;
        }

        public bool InRange(Vector2Int target, int xRange, int yRange)
        {
            Vector2Int rPosition = GetRealPosition();
            return Mathf.Abs(target.x-rPosition.x) <= xRange && Mathf.Abs(target.y-rPosition.y) <= yRange;
        }
        /// <summary> 
        /// loads chunkpartition into tilegridmaps at given angle
        /// </summary>
        public virtual IEnumerator Load(Dictionary<TileMapType, IWorldTileMap> tileGridMaps, Direction direction,
            Vector2Int systemOffset) {
            foreach (IWorldTileMap tileGridMap in tileGridMaps.Values) {
                UnityEngine.Vector2Int realPartitionPosition = GetRealPosition();
                if (!tileGridMap.containsPartition(realPartitionPosition)) {
                    tileGridMap.addPartition(this);
                }
            }
            
            baseTileHardnessArray = new int[Global.ChunkPartitionSize, Global.ChunkPartitionSize];
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            Vector2Int realPosition = GetRealPosition();

            switch (direction) {
                case Direction.Left:
                    for (int x = Global.ChunkPartitionSize-1; x >= 0; x --) {
                        for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                            iterateLoad(x,y,itemRegistry,tileGridMaps,realPosition);
                        }
                        yield return new WaitForEndOfFrame();
                    }
                    break;
                case Direction.Right:
                    for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                        for (int y = 0; y < Global.ChunkPartitionSize ; y ++) {
                            iterateLoad(x,y,itemRegistry,tileGridMaps,realPosition);
                        }
                        yield return new WaitForEndOfFrame();
                    }
                    break;
                case Direction.Up:
                    for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                        for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                            iterateLoad(x,y,itemRegistry,tileGridMaps,realPosition);
                        }
                        yield return new WaitForEndOfFrame();
                    }
                    break;
                case Direction.Down:
                    for (int y = Global.ChunkPartitionSize-1; y >= 0; y --) {
                        for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                            iterateLoad(x,y,itemRegistry,tileGridMaps,realPosition);
                        }
                        yield return new WaitForEndOfFrame();
                    }
                    break;
            }
            
            SetTileLoaded(true);
            yield return null;
        }

        protected abstract void iterateLoad(int x, int y,ItemRegistry itemRegistry, Dictionary<TileMapType, IWorldTileMap> tileGridMaps, Vector2Int realPosition);

        public abstract void Save();

        public void SetScheduleForUnloading(bool val)
        {
            scheduledForUnloading = val;
        }

        public virtual IEnumerator UnloadTiles(Dictionary<TileMapType, IWorldTileMap> tileGridMaps) {
            Save();
            Vector2Int realPosition = GetRealPosition();
            foreach (IWorldTileMap tileMap in tileGridMaps.Values) {
                yield return tileMap.removePartition(realPosition);
            }
        }
        public void UnloadEntities() {
            int size = Global.ChunkPartitionSize/2;
            Vector2 castPosition = (GetRealPosition()+Vector2.one/2f) * size;
            RaycastHit2D[] hits = Physics2D.BoxCastAll(
                castPosition, 
                new Vector2(size,size),
                0f, 
                Vector2.zero, 
                Mathf.Infinity, 
                1 << LayerMask.NameToLayer("Entity")
            );
            List<SeralizedEntityData> entityData = new List<SeralizedEntityData>();
            foreach (RaycastHit2D hit in hits) {
                Entity entity = hit.collider.gameObject.GetComponent<Entity>();
                if (entity is ISerializableEntity serializableEntity) {
                    entityData.Add(serializableEntity.serialize());
                }
                GameObject.Destroy(hit.collider.gameObject);
            }
            data.entityData = entityData;
        }
        public virtual IEnumerator unload(Dictionary<TileMapType, IWorldTileMap> tileGridMaps) {
            yield return UnloadTiles(tileGridMaps);
        }

        public void AddTileEntity(TileMapLayer layer,ITileEntityInstance tileEntity,Vector2Int positionInPartition)
        {
            if (layer != TileMapLayer.Base) return;
            if (tileEntity is ILoadableTileEntity entity) {
                entity.load();
            }
            tileEntities[positionInPartition.x,positionInPartition.y] = tileEntity;
            if (tileEntity is ITickableTileEntity tickableTileEntity) {
                tickableTileEntities.Add(tickableTileEntity);
            }
        }

        public void BreakTileEntity(TileMapLayer layer, Vector2Int position)
        {
            if (layer != TileMapLayer.Base) {
                return;
            }
            ITileEntityInstance tileEntity = tileEntities[position.x,position.y];
            if (tileEntity is IBreakActionTileEntity breakActionTileEntity) {
                breakActionTileEntity.onBreak();
            }
            if (tileEntity is ILoadableTileEntity loadableTileEntity) {
                loadableTileEntity.unload();
            }
            
            tileEntities[position.x,position.y] = null;
            if (tileEntity is ITickableTileEntity tickableTileEntity) {
                tickableTileEntities.Remove(tickableTileEntity);
            }
            
        }

        public bool ClickTileEntity(Vector2Int position)
        {
            ITileEntityInstance tileEntity = tileEntities[position.x,position.y];
            if (tileEntity == null) {
                return false;
            }
            if (tileEntity is IRightClickableTileEntity rightClickableTileEntity) {
                rightClickableTileEntity.onRightClick();
                return true;
            }
            return false;
        }

        public bool GetLoaded()
        {
            return loaded;
        }

        public void SetTileLoaded(bool val)
        {
            loaded = val;
        }
        
        public ITileEntityInstance GetTileEntity(Vector2Int position)
        {
            if (tileEntities == null) {
                return null;
            }
            return tileEntities[position.x,position.y];
        }

        public abstract TileItem GetTileItem(Vector2Int position, TileMapLayer layer);

        public abstract void SetTile(Vector2Int position, TileMapLayer layer, TileItem tileItem);

        public abstract PartitionFluidData GetFluidData();
        public abstract bool GetFarLoaded();
        public abstract void LoadFarLoadTileEntities();
        public abstract void UnloadFarLoadTileEntities();

        public void SetFarLoaded(bool state)
        {
            this.farLoaded = state;
        }

        public bool GetScheduledForFarLoading()
        {
            return scheduledForFarLoading;
        }

        public void SetScheduledForFarLoading(bool state)
        {
            this.scheduledForFarLoading = state;
        }

        public BaseTileData GetBaseData(Vector2Int positionInPartition)
        {
            if (data.baseData.sTileOptions[positionInPartition.x, positionInPartition.y] == null)
            {
                data.baseData.sTileOptions[positionInPartition.x, positionInPartition.y] = new BaseTileData(0, 0, false);
            }
            return data.baseData.sTileOptions[positionInPartition.x,positionInPartition.y];
        }

        

        public bool DeIncrementHardness(Vector2Int positionInPartition)
        {
            baseTileHardnessArray[positionInPartition.x, positionInPartition.y]--;
            return baseTileHardnessArray[positionInPartition.x, positionInPartition.y] == 0;
        }

        public int GetHardness(Vector2Int positionInPartition)
        {
            return baseTileHardnessArray[positionInPartition.x, positionInPartition.y];
        }
    }
}