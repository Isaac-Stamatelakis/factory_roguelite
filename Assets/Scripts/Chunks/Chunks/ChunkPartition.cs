using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;

namespace ChunkModule {
    public interface IChunkPartition {
        public ChunkPartitionData getData();
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
    public abstract class ChunkPartition<T> : IChunkPartition where T : ChunkPartitionData
    {
        protected bool tileLoaded = false;
        protected bool entityLoaded = false;
        protected bool scheduledForUnloading = false;
        protected Vector2Int position;
        protected T data;
        public Dictionary<TileMapLayer, TileEntity[,]> tileEntities;
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

        public ChunkPartitionData getData()
        {
            return data;
        }

        public bool getTileLoaded()
        {
            return tileLoaded;
        }

        public void tick() {
            foreach (TileEntity[,] tileEntitieList in tileEntities.Values) {
                foreach (TileEntity tileEntity in tileEntitieList) {
                    if (tileEntity is ITickableTileEntity) {
                        ((ITickableTileEntity) tileEntity).tickUpdate();
                    }
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
            foreach (TileGridMap tileGridMap in tileGridMaps.Values) {
                UnityEngine.Vector2Int realPartitionPosition = getRealPosition();
                if (!tileGridMap.containsPartition(realPartitionPosition)) {
                    tileGridMap.initPartition(realPartitionPosition);
                }
            }
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            Vector2Int realPosition = getRealPosition();
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
            yield return null;
        }

        protected abstract void iterateLoad(int x, int y,ItemRegistry itemRegistry, Dictionary<TileMapType, ITileMap> tileGridMaps, Vector2Int realPosition);

        public abstract void save(Dictionary<TileMapType, ITileMap> tileGridMaps);

        public void setTileLoaded(bool val)
        {
            tileLoaded = val;
        }

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
            tileEntities = null;
        }

        public bool getEntityLoaded()
        {
            return entityLoaded;
        }

        public void setEntityLoaded(bool val)
        {
            this.entityLoaded = val;
        }

        public void addTileEntity(TileMapLayer layer,TileEntity tileEntity,Vector2Int positionInPartition)
        {
            if (layer == TileMapLayer.Base || layer == TileMapLayer.Background) {
                if (tileEntity is ILoadableTileEntity) {
                    ((ILoadableTileEntity) tileEntity).load();
                }
                tileEntities[layer][positionInPartition.x,positionInPartition.y] = tileEntity;
            }
        }

        public void breakTileEntity(TileMapLayer layer, Vector2Int position)
        {
            if (tileEntities.ContainsKey(layer)) {
                TileEntity tileEntity = tileEntities[layer][position.x,position.y];
                if (tileEntity is IBreakActionTileEntity) {
                    ((IBreakActionTileEntity) tileEntity).onBreak();
                }
                if (tileEntity is ILoadableTileEntity) {
                    ((ILoadableTileEntity) tileEntity).unload();
                }
                tileEntities[layer][position.x,position.y] = null;
            }
        }

        public bool clickTileEntity(TileMapLayer layer, Vector2Int position)
        {
            if (tileEntities.ContainsKey(layer)) {
                TileEntity tileEntity = tileEntities[layer][position.x,position.y];
                if (tileEntity == null) {
                    return false;
                }
                if (tileEntity is IClickableTileEntity) {
                    ((IClickableTileEntity) tileEntity).onClick();
                    return true;
                }
            }
            return false;
        }
    }

    public class TileChunkPartition<T> : ChunkPartition<SerializedTileData> where T : SerializedTileData
    {
        public TileChunkPartition(SerializedTileData data, UnityEngine.Vector2Int position, Chunk parent) : base(data, position, parent)
        {

        }
        public override IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps, double angle)
        {
            if (tileEntities == null) {
                tileEntities = new Dictionary<TileMapLayer, TileEntity[,]>();
            }
            if (!tileEntities.ContainsKey(TileMapLayer.Base)) {
                tileEntities[TileMapLayer.Base] =  new TileEntity[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            }
            if (!tileEntities.ContainsKey(TileMapLayer.Background)) {
                tileEntities[TileMapLayer.Background] = new TileEntity[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            }
            yield return base.load(tileGridMaps,angle);
            
        }

        public override void save(Dictionary<TileMapType, ITileMap> tileGridMaps)
        {
            Vector2Int position = getRealPosition();
            SerializedTileData data = (SerializedTileData) getData();
            
            // Clear data
            for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                    data.baseData.ids[x][y] = null;
                    data.baseData.sTileOptions[x][y] = null;
                    data.backgroundData.ids[x][y] = null;
                    data.backgroundData.sTileOptions[x][y] = null;
                }
            }
            
            // Iterate through tilemaps
            foreach (ITileMap tileMap in tileGridMaps.Values) {
                TileMapType tileMapType = tileMap.getType();
                if (!TileMapTypeFactory.typeIsTile(tileMapType)) { // type is valid tile type
                    continue;
                }
                // get layer to serialze in (base or background)
                TileMapLayer layer = TileMapTypeFactory.MapToSerializeLayer(tileMapType);
                IPlacedItemObject[,] tileItemdata = tileMap.getPartitionData(position);
                if (tileItemdata == null) {
                    continue;
                }
                for (int x = 0; x < Global.ChunkPartitionSize; x ++) {
                    for (int y = 0; y < Global.ChunkPartitionSize; y ++) {
                        if (tileItemdata[x,y] != null) {
                            TileData tileData = (TileData) tileItemdata[x,y];
                            TileItem tileItem = (TileItem) tileData.getItemObject();
                            switch (layer) {
                                case TileMapLayer.Base:
                                    data.baseData.ids[x][y] = tileItem.id;
                                    data.baseData.sTileOptions[x][y] = TileOptionFactory.serializeOptions(tileItem.getOptions());
                                    break;
                                case TileMapLayer.Background:
                                    data.backgroundData.ids[x][y] = tileItem.id;
                                    data.backgroundData.sTileOptions[x][y] = TileOptionFactory.serializeOptions(tileItem.getOptions());
                                    break;
                            }
                        }
                    }
                }
            }


            if (tileEntities != null) {
                if (tileEntities.ContainsKey(TileMapLayer.Base)) {
                    TileEntity[,] tempArr = tileEntities[TileMapLayer.Base];
                    for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                        for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                            TileEntity tileEntity = tempArr[x,y];
                            if (tileEntity == null) {
                                continue;
                            }
                            if (tileEntity is ISerializableTileEntity) {
                                data.baseData.sTileEntityOptions[x][y] = ((ISerializableTileEntity) tileEntity).serialize();
                            }
                        }
                    }
                }
                if (tileEntities.ContainsKey(TileMapLayer.Background)) {
                    TileEntity[,] tempArr = tileEntities[TileMapLayer.Background];
                    for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                        for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                            TileEntity tileEntity = tempArr[x,y];
                            if (tileEntity == null) {
                                continue;
                            }
                            if (tileEntity is ISerializableTileEntity) {
                                data.backgroundData.sTileEntityOptions[x][y] = ((ISerializableTileEntity) tileEntity).serialize();
                            }
                            
                        }
                    }
                }
            }
            
            
        }

        public override IEnumerator unload(Dictionary<TileMapType, ITileMap> tileGridMaps) {
            yield return base.unload(tileGridMaps);
        }

        public override IEnumerator unloadTiles(Dictionary<TileMapType, ITileMap> tileGridMaps) {
            yield return base.unloadTiles(tileGridMaps);
            if (tileEntities != null) {
                int removalsPerNumeration = 5;
                int removals = 0;
                if (tileEntities.ContainsKey(TileMapLayer.Base)) {
                    TileEntity[,] baseTileEntities = tileEntities[TileMapLayer.Base];
                    for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                        for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                            TileEntity baseTileEntity = baseTileEntities[x,y];
                            if (baseTileEntity is ILoadableTileEntity) {
                                ((ILoadableTileEntity) baseTileEntity).unload();
                                baseTileEntities[x,y] = null;
                                removals++;
                            }
                            if (removals >= removalsPerNumeration) {
                                removals = 0;
                                yield return new WaitForEndOfFrame();
                            }
                        }
                    }
                }
                if (tileEntities.ContainsKey(TileMapLayer.Background)) {
                    TileEntity[,] backgroundTileEntities = tileEntities[TileMapLayer.Background];
                    for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                        for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                            TileEntity backgroundTileEntity = backgroundTileEntities[x,y];
                            if (backgroundTileEntity is ILoadableTileEntity) {
                                ((ILoadableTileEntity) backgroundTileEntity).unload();
                                backgroundTileEntities[x,y] = null;
                                removals++;
                            }
                            if (removals >= removalsPerNumeration) {
                                removals = 0;
                                yield return new WaitForEndOfFrame();
                            }
                        }
                    }
                }
                
            }
            
            
            
        }

        protected override void iterateLoad(int x, int y,ItemRegistry itemRegistry, Dictionary<TileMapType, ITileMap> tileGridMaps, Vector2Int realPosition) {
            Vector2Int partitionPosition = new Vector2Int(x,y);
            string baseId = data.baseData.ids[x][y];
            if (baseId != null) {
                Dictionary<string,object> baseOptions = data.baseData.sTileOptions[x][y];
                string baseTileEntityOptions = data.baseData.sTileEntityOptions[x][y];
                place(
                    id: baseId,
                    tileOptions: baseOptions,
                    tileEntityOptions: baseTileEntityOptions,
                    itemRegistry: itemRegistry,
                    tileGridMaps: tileGridMaps,
                    realPosition: realPosition,
                    positionInPartition: partitionPosition,
                    TileMapLayer.Base
                );
            }
            string backgroundID = data.backgroundData.ids[x][y];
            if (backgroundID != null) {
                Dictionary<string,object> backgroundOptions = data.baseData.sTileOptions[x][y];
                string backgroundTileEntityOptions = data.backgroundData.sTileEntityOptions[x][y];
                place(
                    id: backgroundID,
                    tileOptions: backgroundOptions,
                    tileEntityOptions: backgroundTileEntityOptions,
                    itemRegistry: itemRegistry,
                    tileGridMaps: tileGridMaps,
                    realPosition: realPosition,
                    positionInPartition: partitionPosition,
                    TileMapLayer.Background
                );
            }
        }

        protected void place(string id, Dictionary<string,object> tileOptions, string tileEntityOptions,ItemRegistry itemRegistry, Dictionary<TileMapType, ITileMap> tileGridMaps,Vector2Int realPosition,Vector2Int positionInPartition,TileMapLayer layer) {
            TileItem tileItem = itemRegistry.getTileItem(id);
            if (tileItem.tileEntity != null) {
                TileEntity tileEntity = GameObject.Instantiate(tileItem.tileEntity);
                tileEntity.initalize(this.position * Global.ChunkPartitionSize+ positionInPartition,this.parent);
                if (tileEntity is ISerializableTileEntity) {
                    ((ISerializableTileEntity) tileEntity).unserialize(tileEntityOptions);
                }
                if (tileEntity is ILoadableTileEntity) {
                    ((ILoadableTileEntity) tileEntity).load();
                }
                if (layer == TileMapLayer.Base || layer == TileMapLayer.Background) {
                    tileEntities[layer][positionInPartition.x,positionInPartition.y] = tileEntity;
                }
            }
            
            Dictionary<TileItemOption,object> options = tileItem.getOptions();
            TileData tileData = new TileData(
                tileItem,
                options
            );
            ITileMap tileGridMap = tileGridMaps[TileMapTypeFactory.tileToMapType(tileItem.tileType)];
            tileGridMap.placeTileAtLocation(
                realPosition,
                positionInPartition,
                tileData
            );
            
        }
    }

    public class ConduitChunkPartition<T> : TileChunkPartition<SerializedTileConduitData> where T : SerializedTileConduitData
    {
        public ConduitChunkPartition(SerializedTileConduitData data, UnityEngine.Vector2Int position, Chunk parent) : base(data, position, parent)
        {
        }

        public override IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps,double angle)
        {
            return base.load(tileGridMaps,angle);
        }
    }
}