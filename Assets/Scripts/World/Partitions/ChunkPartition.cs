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
using Entities.Mobs;
using Player;

namespace Chunks.Partitions {
    
    public abstract class ChunkPartition<T> : IChunkPartition where T : SeralizedWorldData
    {
        protected bool loaded;
        protected bool loading;
        protected bool farLoaded;
        protected bool scheduledForUnloading = false;
        protected bool scheduledForFarLoading = false;
        protected Vector2Int position;
        protected T data;
        
        public Dictionary<Vector2Int,ITileEntityInstance> tileEntities;
        protected int[,] baseTileHardnessArray;
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
                tileEntity.TickUpdate();
            }
        }

        public UnityEngine.Vector2Int GetRealPosition()
        {
            return parent.GetPosition()*Global.PARTITIONS_PER_CHUNK + position;
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
        public virtual IEnumerator Load(Dictionary<TileMapType, IWorldTileMap> tileGridMaps, Direction direction)
        {
            loading = true;
            foreach (IWorldTileMap tileGridMap in tileGridMaps.Values) {
                UnityEngine.Vector2Int realPartitionPosition = GetRealPosition();
                if (!tileGridMap.containsPartition(realPartitionPosition)) {
                    tileGridMap.addPartition(this);
                }
            }
            
            baseTileHardnessArray = new int[Global.CHUNK_PARTITION_SIZE, Global.CHUNK_PARTITION_SIZE];
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            Vector2Int realPosition = GetRealPosition();

            switch (direction) {
                case Direction.Left:
                    for (int x = Global.CHUNK_PARTITION_SIZE-1; x >= 0; x --) {
                        for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y ++) {
                            iterateLoad(x,y,itemRegistry,tileGridMaps,realPosition);
                        }
                        yield return null;
                    }
                    break;
                case Direction.Right:
                    for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x ++) {
                        for (int y = 0; y < Global.CHUNK_PARTITION_SIZE ; y ++) {
                            iterateLoad(x,y,itemRegistry,tileGridMaps,realPosition);
                        }
                        yield return null;
                    }
                    break;
                case Direction.Up:
                    for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y ++) {
                        for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x ++) {
                            iterateLoad(x,y,itemRegistry,tileGridMaps,realPosition);
                        }
                        yield return null;
                    }
                    break;
                case Direction.Down:
                    for (int y = Global.CHUNK_PARTITION_SIZE-1; y >= 0; y --) {
                        for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x ++) {
                            iterateLoad(x,y,itemRegistry,tileGridMaps,realPosition);
                        }
                        yield return null;
                    }
                    break;
            }
            loading = false;
            SetTileLoaded(true);
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
                if (loading)
                {
                    yield break;
                }
                yield return tileMap.removePartition(realPosition);
                
            }
        }
        public void UnloadEntities() {
            int size = Global.CHUNK_PARTITION_SIZE/2;
            Vector2 castPosition = (GetRealPosition()+Vector2.one/2f) * size;
            RaycastHit2D[] hits = Physics2D.BoxCastAll(
                castPosition, 
                new Vector2(size,size),
                0f, 
                Vector2.zero, 
                Mathf.Infinity, 
                1 << LayerMask.NameToLayer("Entity")
            );
            data.entityData ??= new List<SeralizedEntityData>();
            foreach (RaycastHit2D hit in hits) {
                Entity entity = hit.collider.gameObject.GetComponent<Entity>();
                if (!entity) continue;
                if (entity is MobEntity mobEntity && !mobEntity.RayCastUnLoadable) continue;
                if (entity is ISerializableEntity serializableEntity) {
                    data.entityData.Add(serializableEntity.serialize());
                }
                GameObject.Destroy(hit.collider.gameObject);
            }
        }
        public virtual IEnumerator unload(Dictionary<TileMapType, IWorldTileMap> tileGridMaps) {
            yield return UnloadTiles(tileGridMaps);
        }

        public void AddTileEntity(TileMapLayer layer,ITileEntityInstance tileEntity,Vector2Int positionInPartition)
        {
            if (layer != TileMapLayer.Base) return;
            tileEntities[positionInPartition] = tileEntity;
            if ( tileEntity is ITickableTileEntity tickableTileEntity) {
                tickableTileEntities?.Add(tickableTileEntity);
            }
        }

        public void BreakTileEntity(TileMapLayer layer, Vector2Int positionInPartition)
        {
            if (layer != TileMapLayer.Base) {
                return;
            }

            if (!tileEntities.TryGetValue(positionInPartition, out ITileEntityInstance tileEntity)) return;
            if (tileEntity is IBreakActionTileEntity breakActionTileEntity) {
                breakActionTileEntity.OnBreak();
            }
            if (tileEntity is ILoadableTileEntity loadableTileEntity) {
                loadableTileEntity.Unload();
            }

            tileEntities.Remove(positionInPartition);
            if (tileEntity is ITickableTileEntity tickableTileEntity) {
                tickableTileEntities?.Remove(tickableTileEntity);
            }
            
        }

        public bool GetLoaded()
        {
            return loaded;
        }

        public void SetTileLoaded(bool val)
        {
            loaded = val;
        }
        
        public ITileEntityInstance GetTileEntity(Vector2Int positionInPartition)
        {
            if (tileEntities == null) return null;
            tileEntities.TryGetValue(positionInPartition, out ITileEntityInstance tileEntity);
            return tileEntity;
        }

        public List<TTileEntityType> GetTileEntitiesOfType<TTileEntityType>()
        {
            List<TTileEntityType> typedTileEntities = new List<TTileEntityType>();
            for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x++)
            {
                for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++)
                {
                    if (!tileEntities.TryGetValue(new Vector2Int(x, y), out ITileEntityInstance tileEntityInstance)) continue;
                    if (tileEntityInstance is TTileEntityType tileEntity)
                    {
                        typedTileEntities.Add(tileEntity);
                    }
                }
            }
            return typedTileEntities;
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

        public void UnloadTileEntities()
        {
            if (tileEntities == null) return;
            
            for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x++)
            {
                for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++)
                {
                    if (!tileEntities.TryGetValue(new Vector2Int(x, y), out ITileEntityInstance tileEntity)) continue;
                    if (tileEntity is not ILoadableTileEntity loadableTileEntity) continue;
                    loadableTileEntity.Unload();
                }
            }
        }

        public void SetHardness(Vector2Int positionInPartition, int hardness)
        {
            baseTileHardnessArray[positionInPartition.x, positionInPartition.y] = hardness;
        }

        public void SetBaseTileData(Vector2Int positionInPartition, BaseTileData baseTileData)
        {
            data.baseData.sTileOptions[positionInPartition.x, positionInPartition.y] = baseTileData;
        }
    }
}