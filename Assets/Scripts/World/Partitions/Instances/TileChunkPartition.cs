using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMaps.Type;
using TileMaps;
using TileMaps.Layer;
using TileEntityModule;
using Tiles;
using UnityEngine.Tilemaps;
using Items;
using Conduits.Ports;
using Fluids;
using Entities;

namespace Chunks.Partitions {
public class TileChunkPartition<T> : ChunkPartition<SeralizedWorldData> where T : SeralizedWorldData
    {
        public TileChunkPartition(SeralizedWorldData data, UnityEngine.Vector2Int position, IChunk parent) : base(data, position, parent)
        {

        }
        public override IEnumerator load(Dictionary<TileMapType, ITileMap> tileGridMaps, double angle,Vector2Int systemOffset)
        {
            if (tileEntities == null) {
                tileEntities = new ITileEntityInstance[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            }
            fluidTileMap = (FluidTileMap)tileGridMaps[TileMapType.Fluid];
            yield return base.load(tileGridMaps,angle,systemOffset);
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
                        ITileEntityInstance tileEntity = tileEntities[x,y];
                        if (tileEntity == null) {
                            continue;
                        }
                        if (tileEntity is ISerializableTileEntity serializableTileEntity) {
                            data.baseData.sTileEntityOptions[x,y] = serializableTileEntity.serialize();
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
                    
                for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                    for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                        ITileEntityInstance tileEntity = tileEntities[x,y];
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

        protected virtual bool unloadTileEntity(ITileEntityInstance[,] array, int x, int y) {
            ITileEntityInstance tileEntityInstance = array[x,y];
            if (tileEntityInstance == null || tileEntityInstance.GetTileEntity().SoftLoadable) {
                return false;
            }
            if (tileEntityInstance is ILoadableTileEntity loadableTileEntity) {
                loadableTileEntity.unload();
            }
            if (tileEntityInstance is ISerializableTileEntity serializableTileEntity) {
                data.baseData.sTileEntityOptions[x,y] = serializableTileEntity.serialize();
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

        protected virtual void placeTileEntityFromLoad(TileItem tileItem, string options, Vector2Int positionInPartition, ITileEntityInstance[,] tileEntityArray, int x, int y) {
            
            ITileEntityInstance tileEntityInstance = tileEntityArray[x,y];
            if (tileEntityInstance != null) {
                if (tileEntityInstance is ILoadableTileEntity loadableTileEntity) {
                    loadableTileEntity.load();
                }
                return;
            }
            Vector2Int position = this.position * Global.ChunkPartitionSize + positionInPartition;
            tileEntityArray[x,y] = TileEntityHelper.placeTileEntity(tileItem,position,parent,true,true,options);
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

        public override PartitionFluidData getFluidData()
        {
            SeralizedWorldData serializedTileData = (SeralizedWorldData) getData();
            return new PartitionFluidData(serializedTileData.fluidData.ids,serializedTileData.baseData.ids,serializedTileData.fluidData.fill);
        }

        public override bool getFarLoaded()
        {
            return farLoaded;
        }

        public override void loadFarLoadTileEntities()
        {
            if (tileEntities == null || farLoaded) {
                return;
            }
            farLoaded = true;
            SeralizedWorldData data = (SeralizedWorldData) getData();
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            string[,] tileIds = data.baseData.ids;
            for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                    if (tileEntities[x,y] != null) {
                        continue;
                    }
                    string id = data.baseData.ids[x,y];
                    if (id == null) {
                        continue;
                    }
                    TileItem tileItem = itemRegistry.getTileItem(id);
                    if (tileItem == null) {
                        continue;
                    }
                    TileEntity tileEntity = tileItem.tileEntity;
                    if (tileEntity == null || !tileEntity.ExtraLoadRange) {
                        continue;
                    }
                    string tileEntityData = data.baseData.sTileEntityOptions[x,y];
                    Vector2Int position = this.position * Global.ChunkPartitionSize + new Vector2Int(x,y);
                    tileEntities[x,y] = TileEntityHelper.placeTileEntity(tileItem,position,parent,true,true,tileEntityData);
                }
            }
        }

        public override void unloadFarLoadTileEntities()
        {
            if (tileEntities == null || !farLoaded) {
                return;
            }
            farLoaded = false;
            for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                    ITileEntityInstance tileEntityInstance = tileEntities[x,y];
                    if (tileEntityInstance == null) {
                        continue;
                    }
                    TileEntity tileEntity = tileEntityInstance.GetTileEntity();
                    if (tileEntity.ExtraLoadRange) {
                        unloadTileEntity(tileEntities,x,y);
                    }
                }
            }
        }
    }
}