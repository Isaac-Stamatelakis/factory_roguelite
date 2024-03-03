using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using TileMapModule;
using TileMapModule.Layer;
using TileMapModule.Type;
using Tiles;
using ItemModule;

namespace ChunkModule.PartitionModule {
    
    public abstract class ChunkPartition<T> : IChunkPartition where T : IChunkPartitionData
    {
        protected bool loaded;
        protected bool scheduledForUnloading = false;
        protected Vector2Int position;
        protected T data;
        public TileEntity[,] tileEntities;
        public TileOptions[,] tileOptionsArray;
        protected Chunk parent;

        public ChunkPartition(T data, Vector2Int position, Chunk parent) {
            this.data = data;
            this.position = position;
            this.parent = parent;
        }

        public float distanceFrom(Vector2Int target)
        {
            Vector2Int realPosition = getRealPosition();
            return Mathf.Pow(target.x-realPosition.x,2) + Mathf.Pow(target.y-realPosition.y,2);
        }

        public IChunkPartitionData getData()
        {
            return data;
        }


        public void tick() {
            if (tileEntities == null) {
                return;
            }
            foreach (TileEntity tileEntity in tileEntities) {
                if (tileEntity is ITickableTileEntity) {
                    ((ITickableTileEntity) tileEntity).tickUpdate();
                }
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
        public virtual IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps,double angle) {
            tileOptionsArray = new TileOptions[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            foreach (ITileMap tileGridMap in tileGridMaps.Values) {
                UnityEngine.Vector2Int realPartitionPosition = getRealPosition();
                if (!tileGridMap.containsPartition(realPartitionPosition)) {
                    tileGridMap.addPartition(this);
                }
            }
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            Vector2Int realPosition = getRealPosition();

            
            for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                    iterateLoad(x,y,itemRegistry,tileGridMaps,realPosition);
                }
                yield return new WaitForEndOfFrame();
            }
            /*
            if (angle > 45 && angle <= 135) { // up
                for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                    for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                        iterateLoad(x,y,itemRegistry,tileGridMaps,realPosition);
                    }
                    yield return new WaitForEndOfFrame();
                }
            } else if (angle <= 225) { // left
                for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                    for (int x = Global.ChunkPartitionSize-1; x >=0 ; x --) {
                        iterateLoad(x,y,itemRegistry,tileGridMaps,realPosition);
                    }
                    yield return new WaitForEndOfFrame();
                }
            } else if (angle <= 315) { // down
                for (int y = Global.ChunkPartitionSize-1; y >= 0; y --) {
                    for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                        iterateLoad(x,y,itemRegistry,tileGridMaps,realPosition);
                    }
                    yield return new WaitForEndOfFrame();
                }
            } else { // right
                for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                    for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                        iterateLoad(x,y,itemRegistry,tileGridMaps,realPosition);
                    }
                    yield return new WaitForEndOfFrame();
                }
            }
            */
            yield return null;
        }

        protected abstract void iterateLoad(int x, int y,ItemRegistry itemRegistry, Dictionary<TileMapType, ITileMap> tileGridMaps, Vector2Int realPosition);

        public abstract void save(Dictionary<TileMapType, ITileMap> tileGridMaps);



        public void setScheduleForUnloading(bool val)
        {
            scheduledForUnloading = val;
        }

        public virtual IEnumerator unloadTiles(Dictionary<TileMapType, ITileMap> tileGridMaps) {
            save(tileGridMaps);
            unloadEntities();
            Vector2Int realPosition = getRealPosition();
            foreach (ITileMap tileMap in tileGridMaps.Values) {
                yield return tileMap.removePartition(realPosition);
            }
        }

        protected void unloadEntities() {
            Vector2 castPosition = (getRealPosition() + Vector2Int.one/2) * Global.ChunkPartitionSize/2;
            RaycastHit2D[] hits = Physics2D.BoxCastAll(castPosition, new Vector2(Global.ChunkPartitionSize,Global.ChunkPartitionSize), 0f, Vector2.zero, Mathf.Infinity, 1 << LayerMask.NameToLayer("Entity"));
            List<EntityData> entityDatas = new List<EntityData>();
            foreach (RaycastHit2D hit in hits) {
                Entity entity = hit.collider.gameObject.GetComponent<Entity>();
                entityDatas.Add(entity.GetData());
            }
        }
        public virtual IEnumerator unload(Dictionary<TileMapType, ITileMap> tileGridMaps) {
            yield return unloadTiles(tileGridMaps);
            unloadTileEntities();
        }

        protected virtual void unloadTileEntities() {
            tileEntities = null;
        }
        public void loadEntities() {

        }
        public void addTileEntity(TileMapLayer layer,TileEntity tileEntity,Vector2Int positionInPartition)
        {
            if (layer == TileMapLayer.Base) {
                if (tileEntity is ILoadableTileEntity) {
                    ((ILoadableTileEntity) tileEntity).load();
                }
                tileEntities[positionInPartition.x,positionInPartition.y] = tileEntity;
            }
        }

        public void breakTileEntity(TileMapLayer layer, Vector2Int position)
        {
            if (layer != TileMapLayer.Base) {
                return;
            }
            TileEntity tileEntity = tileEntities[position.x,position.y];
            if (tileEntity is IBreakActionTileEntity) {
                ((IBreakActionTileEntity) tileEntity).onBreak();
            }
            if (tileEntity is ILoadableTileEntity) {
                ((ILoadableTileEntity) tileEntity).unload();
            }
            tileEntities[position.x,position.y] = null;
            
        }

        public bool clickTileEntity(Vector2Int position)
        {
            TileEntity tileEntity = tileEntities[position.x,position.y];
            if (tileEntity == null) {
                return false;
            }
            if (tileEntity is IClickableTileEntity) {
                ((IClickableTileEntity) tileEntity).onClick();
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
            return tileOptionsArray[position.x,position.y];
        }


        public TileEntity GetTileEntity(Vector2Int position)
        {
            return tileEntities[position.x,position.y];
        }

        public abstract TileItem GetTileItem(Vector2Int position, TileMapLayer layer);

        public abstract void setTile(Vector2Int position, TileMapLayer layer, TileItem tileItem);
    }
}