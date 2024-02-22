using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapModule.Type;
using TileMapModule;
using TileMapModule.Layer;
using TileEntityModule;
using Tiles;

namespace ChunkModule.PartitionModule {
public class TileChunkPartition<T> : ChunkPartition<SerializedTileData> where T : SerializedTileData
    {
        public TileChunkPartition(SerializedTileData data, UnityEngine.Vector2Int position, Chunk parent) : base(data, position, parent)
        {

        }
        public override IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps, double angle)
        {
            if (tileEntities == null) {
                tileEntities = new TileEntity[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            }
            yield return base.load(tileGridMaps,angle);
            
        }

        public override void save(Dictionary<TileMapType, ITileMap> tileGridMaps)
        {
            Vector2Int position = getRealPosition();
            SerializedTileData data = (SerializedTileData) getData();
            if (tileOptionsArray != null) {
                for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                    for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                        TileOptions options = tileOptionsArray[x,y];
                        if (options == null) {
                            continue;
                        }
                        data.baseData.sTileOptions[x][y] = TileOptionFactory.serialize(options);
                    }
                }
            }
            if (tileEntities != null) {
                for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                    for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                        TileEntity tileEntity = tileEntities[x,y];
                        if (tileEntity == null) {
                            continue;
                        }
                        if (tileEntity is ISerializableTileEntity) {
                            data.baseData.sTileEntityOptions[x][y] = saveTileEntity(tileEntity);
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
                    
                for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                    for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                        TileEntity tileEntity = tileEntities[x,y];
                        if (tileEntity == null) {
                            continue;
                        }
                        if(unloadTileEntity(tileEntities,x,y)) {
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
                    tileOptionData: baseOptions,
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
                    tileOptionData: backgroundOptions,
                    tileEntityOptions: backgroundTileEntityOptions,
                    itemRegistry: itemRegistry,
                    tileGridMaps: tileGridMaps,
                    realPosition: realPosition,
                    positionInPartition: partitionPosition,
                    TileMapLayer.Background
                );
            }
        }

        private void place(string id, string tileOptionData, string tileEntityOptions,ItemRegistry itemRegistry, Dictionary<TileMapType, ITileMap> tileGridMaps,Vector2Int realPosition,Vector2Int positionInPartition,TileMapLayer layer) {
            TileItem tileItem = itemRegistry.getTileItem(id);
            if (tileItem == null) {
                return;
            }
            if (tileItem.tileEntity != null) {
                if (layer == TileMapLayer.Base) {
                    placeTileEntityFromLoad(
                        tileItem,
                        tileEntityOptions,
                        positionInPartition,
                        tileEntities,
                        positionInPartition.x,
                        positionInPartition.y
                    );
                }
            }
            TileOptions options = TileOptionFactory.deserialize(tileOptionData,tileItem);
            tileOptionsArray[positionInPartition.x,positionInPartition.y] = options;
            ITileMap tileGridMap = tileGridMaps[tileItem.tileType.toTileMapType()];
            tileGridMap.placeTileAtLocation(
                realPosition,
                positionInPartition,
                tileItem
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

        public override TileItem GetTileItem(Vector2Int position, TileMapLayer layer)
        {
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            {
                switch (layer) {
                    case TileMapLayer.Base:
                        return itemRegistry.getTileItem(data.baseData.ids[position.x][position.y]);
                    case TileMapLayer.Background:
                        return itemRegistry.getTileItem(data.backgroundData.ids[position.x][position.y]);
                    default:
                        Debug.LogError("TileChunkPartition attempted to return tileitem from invalid layer " + layer.ToString());
                        return null;
                }
            }
        }

        public override void setTile(Vector2Int position, TileMapLayer layer, TileItem tileItem)
        {
            SerializedTileData tileData = (SerializedTileData) getData();
            string id = null;
            if (tileItem != null) {
                id = tileItem.id;
            }
            switch (layer) {
                case TileMapLayer.Base:
                    tileData.baseData.ids[position.x][position.y] = id;
                    break;
                case TileMapLayer.Background:
                    tileData.backgroundData.ids[position.x][position.y] = id;
                    break;  

            }
        }
    }
}