using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapModule.Type;
using TileMapModule;
using TileMapModule.Layer;
using TileEntityModule;
using Tiles;
using UnityEngine.Tilemaps;
using ItemModule;
using ConduitModule.Ports;
using Fluids;
using Entities;

namespace ChunkModule.PartitionModule {
public class TileChunkPartition<T> : ChunkPartition<SeralizedWorldData> where T : SeralizedWorldData
    {
        public TileChunkPartition(SeralizedWorldData data, UnityEngine.Vector2Int position, IChunk parent) : base(data, position, parent)
        {

        }
        public override IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps, double angle)
        {
            if (tileEntities == null) {
                tileEntities = new TileEntity[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            }
            fluidTileMap = (FluidTileMap)tileGridMaps[TileMapType.Fluid];
            yield return base.load(tileGridMaps,angle);
            if (parent is ILoadedChunk loadedChunk) {
                foreach (SeralizedEntityData seralizedEntityData in data.entityData) {
                    EntityUtils.spawnFromData(seralizedEntityData,loadedChunk.getEntityContainer());
                }
            }
            data.entityData = new List<SeralizedEntityData>(); // Prevents duplication
            
        }
        private FluidTileMap fluidTileMap;

        public override void save()
        {
            Vector2Int position = getRealPosition();
            SeralizedWorldData data = (SeralizedWorldData) getData();
            if (tileOptionsArray != null) {
                for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                    for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                        TileOptions options = tileOptionsArray[x,y];
                        data.baseData.sTileOptions[x,y] = TileOptionFactory.serialize(options);
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
                            data.baseData.sTileEntityOptions[x,y] = saveTileEntity(tileEntity);
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
            if (tileEntity is ISoftLoadable) {
                return false;
            }
            if (tileEntity is ILoadableTileEntity) {
                ((ILoadableTileEntity) tileEntity).unload();
            }
            array[x,y] = null;
            return true;
        }

        protected override void iterateLoad(int x, int y,ItemRegistry itemRegistry, Dictionary<TileMapType, ITileMap> tileGridMaps, Vector2Int realPosition) {
            Vector2Int partitionPosition = new Vector2Int(x,y);
            string baseId = data.baseData.ids[x,y];
            if (baseId != null) {
                string baseOptions = data.baseData.sTileOptions[x,y];
                string baseTileEntityOptions = data.baseData.sTileEntityOptions[x,y];
                placeBase(
                    id: baseId,
                    tileOptionData: baseOptions,
                    tileEntityOptions: baseTileEntityOptions,
                    itemRegistry: itemRegistry,
                    tileGridMaps: tileGridMaps,
                    realPosition: realPosition,
                    positionInPartition: partitionPosition
                );
            }
            string backgroundID = data.backgroundData.ids[x,y];
            if (backgroundID != null) {
                placeBackground(
                    id: backgroundID,
                    itemRegistry: itemRegistry,
                    tileGridMaps: tileGridMaps,
                    realPosition: realPosition,
                    positionInPartition: partitionPosition
                );
            }
            string fluidID = data.fluidData.ids[x,y];
            if (fluidID != null) {
                placeFluid(
                    id: fluidID,
                    itemRegistry: itemRegistry,
                    tileGridMaps: tileGridMaps,
                    realPosition: realPosition,
                    positionInPartition: partitionPosition
                );
            }
        }

        private void placeBackground(string id, ItemRegistry itemRegistry, Dictionary<TileMapType, ITileMap> tileGridMaps,Vector2Int realPosition,Vector2Int positionInPartition) {
            TileItem tileItem = itemRegistry.getTileItem(id);
            if (tileItem == null) {
                return;
            }
            ITileMap tileGridMap = tileGridMaps[tileItem.tileType.toTileMapType()];
            tileGridMap.placeItemTileAtLocation(
                realPosition,
                positionInPartition,
                tileItem
            );
            
        }

        private void placeFluid(string id, ItemRegistry itemRegistry, Dictionary<TileMapType, ITileMap> tileGridMaps, Vector2Int realPosition, Vector2Int positionInPartition) {
            FluidTileItem fluidTileItem = itemRegistry.getFluidTileItem(id);
            if (fluidTileItem == null) {
                return;
            }
            ITileMap tileGridMap = tileGridMaps[TileMapType.Fluid];
            tileGridMap.placeItemTileAtLocation(
                realPosition,
                positionInPartition,
                fluidTileItem
            );
        }
        private void placeBase(string id, string tileOptionData, string tileEntityOptions,ItemRegistry itemRegistry, Dictionary<TileMapType, ITileMap> tileGridMaps,Vector2Int realPosition,Vector2Int positionInPartition) {
            TileItem tileItem = itemRegistry.getTileItem(id);
            if (tileItem == null) {
                return;
            }
            TileOptions options = TileOptionFactory.deserialize(tileOptionData,tileItem);
            tileOptionsArray[positionInPartition.x,positionInPartition.y] = options;
            if (tileItem.tileEntity != null ) {
                placeTileEntityFromLoad(
                    tileItem,
                    tileEntityOptions,
                    positionInPartition,
                    tileEntities,
                    positionInPartition.x,
                    positionInPartition.y
                );
            }
            
            TileBase tileBase = tileItem.tile;
            if (tileItem.tile is IStateTile stateTile) {
                tileBase = stateTile.getTileAtState(options.SerializedTileOptions.state);
            }
            ITileMap tileGridMap = tileGridMaps[tileItem.tileType.toTileMapType()];
            tileGridMap.placeItemTileAtLocation(
                realPosition,
                positionInPartition,
                tileItem
            );
        }

        protected virtual void placeTileEntityFromLoad(TileItem tileItem, string options, Vector2Int positionInPartition, TileEntity[,] tileEntityArray, int x, int y) {
            if (tileItem.tileEntity is ISoftLoadable) {
                TileEntity tileEntity = tileEntityArray[x,y];
                if (tileEntity is ILoadableTileEntity loadableTileEntity) {
                    loadableTileEntity.load();
                }
                return;
            }
            tileEntityArray[x,y] = placeTileEntity(tileItem,options,positionInPartition,true);
        }

        protected TileEntity placeTileEntity(TileItem tileItem, string options, Vector2Int positionInPartition, bool load) {
            TileEntity tileEntity = GameObject.Instantiate(tileItem.tileEntity);
            tileEntity.initalize(this.position * Global.ChunkPartitionSize+ positionInPartition,tileItem.tile, this.parent);
            if (tileEntity is ISerializableTileEntity serializableTileEntity) {
                serializableTileEntity.unserialize(options);
            }
            if (load && tileEntity is ILoadableTileEntity loadableTileEntity) {
                loadableTileEntity.load();
            }
            return tileEntity;
        }

        public override TileItem GetTileItem(Vector2Int position, TileMapLayer layer)
        {
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            {
                switch (layer) {
                    case TileMapLayer.Base:
                        return itemRegistry.getTileItem(data.baseData.ids[position.x,position.y]);
                    case TileMapLayer.Background:
                        return itemRegistry.getTileItem(data.backgroundData.ids[position.x,position.y]);
                    default:
                        Debug.LogError("TileChunkPartition attempted to return tileitem from invalid layer " + layer.ToString());
                        return null;
                }
            }
        }

        public override void setTile(Vector2Int position, TileMapLayer layer, TileItem tileItem)
        {
            SeralizedWorldData tileData = (SeralizedWorldData) getData();
            string id = null;
            if (tileItem != null) {
                id = tileItem.id;
            }
            switch (layer) {
                case TileMapLayer.Base:
                    tileData.baseData.ids[position.x,position.y] = id;
                    break;
                case TileMapLayer.Background:
                    tileData.backgroundData.ids[position.x,position.y] = id;
                    return;
                case TileMapLayer.Fluid:
                    tileData.fluidData.ids[position.x,position.y] = id;
                    return;
            }
            if (id == null) {
                tileOptionsArray[position.x,position.y] = null;
                return;
            }
            TileOptions tileOptions = TileOptionFactory.getDefault(tileItem);
            if (tileItem.tile is IRestrictedTile restrictedTile) {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                SerializedTileOptions serializedTileOptions = tileOptions.SerializedTileOptions;
                serializedTileOptions.state = restrictedTile.getStateAtPosition(mousePosition,MousePositionFactory.getVerticalMousePosition(mousePosition),MousePositionFactory.getHorizontalMousePosition(mousePosition));
                tileOptions.SerializedTileOptions = serializedTileOptions;
            }
            tileOptionsArray[position.x,position.y] = tileOptions;
        }

        public override (string[,], string[,], int[,]) getFluidData()
        {
            SeralizedWorldData serializedTileData = (SeralizedWorldData) getData();
            return (serializedTileData.fluidData.ids,serializedTileData.baseData.ids,serializedTileData.fluidData.fill);
        }
    }
}