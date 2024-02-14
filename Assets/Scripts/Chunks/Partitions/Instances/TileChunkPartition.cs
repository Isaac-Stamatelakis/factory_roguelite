using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapModule.Type;
using TileMapModule;
using TileMapModule.Layer;
using TileEntityModule;

namespace ChunkModule.PartitionModule {
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
                if (!tileMapType.isTile()) { // type is valid tile type
                    continue;
                }
                // get layer to serialze in (base or background)
                TileMapLayer layer = tileMapType.toLayer();
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
                                data.baseData.sTileEntityOptions[x][y] = saveTileEntity(tileEntity);
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
                            data.backgroundData.sTileEntityOptions[x][y] = saveTileEntity(tileEntity);
                        }
                    }
                }
            }
        }

        protected virtual string saveTileEntity(TileEntity tileEntity) {
            if (tileEntity is ISerializableTileEntity) {
                return ((ISerializableTileEntity) tileEntity).serialize();
            }
            return null;
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
                            TileEntity tileEntity = baseTileEntities[x,y];
                            if (tileEntity == null) {
                                continue;
                            }
                            if(unloadTileEntity(baseTileEntities,x,y)) {
                                removals ++;
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
                            TileEntity tileEntity = backgroundTileEntities[x,y];
                            if (tileEntity == null) {
                                continue;
                            }
                            if(unloadTileEntity(backgroundTileEntities,x,y)) {
                                removals ++;
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

        protected virtual bool unloadTileEntity(TileEntity[,] array, int x, int y) {
            TileEntity tileEntity = array[x,y];
            if (tileEntity is ILoadableTileEntity) {
                ((ILoadableTileEntity) tileEntity).unload();
            }
            array[x,y] = null;
            return true;
        }


        protected override void iterateLoad(int x, int y,ItemRegistry itemRegistry, Dictionary<TileMapType, ITileMap> tileGridMaps, Vector2Int realPosition) {
            Vector2Int partitionPosition = new Vector2Int(x,y);
            string baseId = data.baseData.ids[x][y];
            if (baseId != null) {
                string baseOptions = data.baseData.sTileOptions[x][y];
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
                string backgroundOptions = data.baseData.sTileOptions[x][y];
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

        private void place(string id, string tileOptions, string tileEntityOptions,ItemRegistry itemRegistry, Dictionary<TileMapType, ITileMap> tileGridMaps,Vector2Int realPosition,Vector2Int positionInPartition,TileMapLayer layer) {
            TileItem tileItem = itemRegistry.getTileItem(id);
            if (tileItem == null) {
                return;
            }
            if (tileItem.tileEntity != null) {
                if (layer == TileMapLayer.Base || layer == TileMapLayer.Background) {
                    placeTileEntityFromLoad(
                        tileItem,
                        tileEntityOptions,
                        positionInPartition,
                        tileEntities[layer],
                        positionInPartition.x,
                        positionInPartition.y
                    );
                }
            }
            
            Dictionary<TileItemOption,object> options = tileItem.getOptions();
            TileData tileData = new TileData(
                tileItem,
                options
            );
            ITileMap tileGridMap = tileGridMaps[tileItem.tileType.toTileMapType()];
            tileGridMap.placeTileAtLocation(
                realPosition,
                positionInPartition,
                tileData
            );
            
        }

        protected virtual void placeTileEntityFromLoad(TileItem tileItem, string options, Vector2Int positionInPartition, TileEntity[,] tileEntityArray, int x, int y) {
            tileEntityArray[x,y] = placeTileEntity(tileItem,options,positionInPartition);
        }

        protected TileEntity placeTileEntity(TileItem tileItem, string options, Vector2Int positionInPartition) {
            TileEntity tileEntity = GameObject.Instantiate(tileItem.tileEntity);
            tileEntity.initalize(this.position * Global.ChunkPartitionSize+ positionInPartition,this.parent);
            if (tileEntity is ISerializableTileEntity) {
                ((ISerializableTileEntity) tileEntity).unserialize(options);
            }
            if (tileEntity is ILoadableTileEntity) {
                ((ILoadableTileEntity) tileEntity).load();
            }
            return tileEntity;
        }
    }
}