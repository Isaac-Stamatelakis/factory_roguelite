using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
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
        protected bool scheduledForUnloading = false;
        protected Vector2Int position;
        protected T data;
        public ITileEntityInstance[,] tileEntities;
        public TileOptions[,] tileOptionsArray;
        public List<ITickableTileEntity> tickableTileEntities;
        protected IChunk parent;

        public ChunkPartition(T data, Vector2Int position, IChunk parent) {
            this.data = data;
            this.position = position;
            this.parent = parent;
        }

        public float distanceFrom(Vector2Int target)
        {
            Vector2Int realPosition = getRealPosition();
            return Mathf.Pow(target.x-realPosition.x,2) + Mathf.Pow(target.y-realPosition.y,2);
        }

        public SeralizedWorldData getData()
        {
            return data;
        }


        public void tick() {
            if (tickableTileEntities == null) {
                return;
            }
            foreach (ITickableTileEntity tileEntity in tickableTileEntities) {
                tileEntity.tickUpdate();
            }
        }

        public UnityEngine.Vector2Int getRealPosition()
        {
            return parent.getPosition()*Global.PartitionsPerChunk + position;
        }

        public bool getScheduledForUnloading()
        {
            return scheduledForUnloading;
        }

        public bool inRange(Vector2Int target, int xRange, int yRange)
        {
            Vector2Int rPosition = getRealPosition();
            return Mathf.Abs(target.x-rPosition.x) <= xRange && Mathf.Abs(target.y-rPosition.y) <= yRange;
        }
        /// <summary> 
        /// loads chunkpartition into tilegridmaps at given angle
        /// </summary>
        public virtual IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps,double angle,Vector2Int systemOffset) {
            tileOptionsArray = new TileOptions[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            tickableTileEntities = new List<ITickableTileEntity>();
            foreach (ITileMap tileGridMap in tileGridMaps.Values) {
                UnityEngine.Vector2Int realPartitionPosition = getRealPosition();
                if (!tileGridMap.containsPartition(realPartitionPosition)) {
                    tileGridMap.addPartition(this);
                }
            }
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            Vector2Int realPosition = getRealPosition();

            /*
            for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                    iterateLoad(x,y,itemRegistry,tileGridMaps,realPosition);
                }
                yield return new WaitForEndOfFrame();
            }
            */

            if (angle > 45 && angle <= 135) { // up
                for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                    for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                        iterateLoad(x,y,itemRegistry,tileGridMaps,realPosition);
                    }
                    yield return new WaitForEndOfFrame();
                }
            } else if (angle <= 225) { // left
                for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                    for (int y = Global.ChunkPartitionSize-1; y >=0 ; y --) {
                        iterateLoad(x,y,itemRegistry,tileGridMaps,realPosition);
                    }
                    yield return new WaitForEndOfFrame();
                }
            } else if (angle <= 315) { // down
                for (int x = Global.ChunkPartitionSize-1; x >= 0; x --) {
                    for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                        iterateLoad(x,y,itemRegistry,tileGridMaps,realPosition);
                    }
                    yield return new WaitForEndOfFrame();
                }
            } else { // right
                for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                    for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                        iterateLoad(x,y,itemRegistry,tileGridMaps,realPosition);
                    }
                    yield return new WaitForEndOfFrame();
                }
            }
            yield return null;
        }

        protected abstract void iterateLoad(int x, int y,ItemRegistry itemRegistry, Dictionary<TileMapType, ITileMap> tileGridMaps, Vector2Int realPosition);

        public abstract void save();

        public void setScheduleForUnloading(bool val)
        {
            scheduledForUnloading = val;
        }

        public virtual IEnumerator unloadTiles(Dictionary<TileMapType, ITileMap> tileGridMaps) {
            save();
            Vector2Int realPosition = getRealPosition();
            foreach (ITileMap tileMap in tileGridMaps.Values) {
                yield return tileMap.removePartition(realPosition);
            }
        }
        public void unloadEntities() {
            int size = Global.ChunkPartitionSize/2;
            Vector2 castPosition = (getRealPosition()+Vector2.one/2f) * size;
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
        public virtual IEnumerator unload(Dictionary<TileMapType, ITileMap> tileGridMaps) {
            yield return unloadTiles(tileGridMaps);
        }

        public void addTileEntity(TileMapLayer layer,ITileEntityInstance tileEntity,Vector2Int positionInPartition)
        {
            if (layer == TileMapLayer.Base) {
                if (tileEntity is ILoadableTileEntity) {
                    ((ILoadableTileEntity) tileEntity).load();
                }
                tileEntities[positionInPartition.x,positionInPartition.y] = tileEntity;
                if (tileEntity is ITickableTileEntity tickableTileEntity) {
                    tickableTileEntities.Add(tickableTileEntity);
                }
            }
        }

        public void breakTileEntity(TileMapLayer layer, Vector2Int position)
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

        public bool clickTileEntity(Vector2Int position)
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

        public bool getLoaded()
        {
            return loaded;
        }

        public void setTileLoaded(bool val)
        {
            loaded = val;
        }

        public TileOptions getTileOptions(Vector2Int position)
        {
            if (tileOptionsArray == null) {
                return null;
            }
            return tileOptionsArray[position.x,position.y];
        }


        public ITileEntityInstance GetTileEntity(Vector2Int position)
        {
            if (tileEntities == null) {
                return null;
            }
            return tileEntities[position.x,position.y];
        }

        public abstract TileItem GetTileItem(Vector2Int position, TileMapLayer layer);

        public abstract void setTile(Vector2Int position, TileMapLayer layer, TileItem tileItem);

        public abstract (string[,], string[,], int[,]) getFluidData();
    }
}