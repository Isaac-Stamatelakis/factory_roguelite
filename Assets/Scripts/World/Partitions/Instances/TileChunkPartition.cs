using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMaps.Type;
using TileMaps;
using TileMaps.Layer;
using TileEntity;
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
        public override IEnumerator Load(Dictionary<TileMapType, IWorldTileMap> tileGridMaps, Direction direction,
            Vector2Int systemOffset)
        {
            tileEntities ??= new ITileEntityInstance[Global.ChunkPartitionSize, Global.ChunkPartitionSize];
            fluidIWorldTileMap = (FluidIWorldTileMap)tileGridMaps[TileMapType.Fluid];
            yield return base.Load(tileGridMaps,direction,systemOffset);
            const int ENTITY_LOAD_PER_UPDATE = 5;
            if (parent is not ILoadedChunk loadedChunk) yield break;
            
            int loads = 0;
            for (int i = data.entityData.Count - 1; i >= 0; i--)
            {
                if (!loaded) yield break;
                EntityUtils.spawnFromData(data.entityData[i],loadedChunk.getEntityContainer());
                data.entityData.RemoveAt(i);
                loads++;
                if (loads >= ENTITY_LOAD_PER_UPDATE)
                {
                    yield return new WaitForFixedUpdate();
                }
            }
        }
        private FluidIWorldTileMap fluidIWorldTileMap;

        public override void Save()
        {
            Vector2Int position = GetRealPosition();
            SeralizedWorldData data = (SeralizedWorldData) GetData();
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

        public override IEnumerator unload(Dictionary<TileMapType, IWorldTileMap> tileGridMaps) {
            yield return base.unload(tileGridMaps);
        }

        public override IEnumerator UnloadTiles(Dictionary<TileMapType, IWorldTileMap> tileGridMaps) {
            yield return base.UnloadTiles(tileGridMaps);
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

        protected override void iterateLoad(int x, int y,ItemRegistry itemRegistry, Dictionary<TileMapType, IWorldTileMap> tileGridMaps, Vector2Int realPosition) {
            Vector2Int partitionPosition = new Vector2Int(x,y);
            string baseId = data.baseData.ids[x,y];
            if (baseId != null) {
                string baseTileEntityOptions = data.baseData.sTileEntityOptions[x,y];
                PlaceBase(
                    id: baseId,
                    tileEntityOptions: baseTileEntityOptions,
                    itemRegistry: itemRegistry,
                    tileGridMaps: tileGridMaps,
                    realPosition: realPosition,
                    positionInPartition: partitionPosition
                );
            }
            string backgroundID = data.backgroundData.ids[x,y];
            if (backgroundID != null) {
                PlaceBackground(
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

        private static void PlaceBackground(string id, ItemRegistry itemRegistry, Dictionary<TileMapType, IWorldTileMap> tileGridMaps,Vector2Int realPosition,Vector2Int positionInPartition) {
            TileItem tileItem = itemRegistry.GetTileItem(id);
            if (ReferenceEquals(tileItem,null)) {
                return;
            }
            IWorldTileMap iWorldTileGridMap = tileGridMaps[tileItem.tileType.toTileMapType()];
            iWorldTileGridMap.placeItemTileAtLocation(
                realPosition,
                positionInPartition,
                tileItem
            );
            
        }

        private static void placeFluid(string id, ItemRegistry itemRegistry, Dictionary<TileMapType, IWorldTileMap> tileGridMaps, Vector2Int realPosition, Vector2Int positionInPartition) {
            FluidTileItem fluidTileItem = itemRegistry.GetFluidTileItem(id);
            if (ReferenceEquals(fluidTileItem,null)) {
                return;
            }
            IWorldTileMap iWorldTileGridMap = tileGridMaps[TileMapType.Fluid];
            iWorldTileGridMap.placeItemTileAtLocation(
                realPosition,
                positionInPartition,
                fluidTileItem
            );
        }
        private void PlaceBase(string id, string tileEntityOptions,ItemRegistry itemRegistry, Dictionary<TileMapType, IWorldTileMap> tileGridMaps,Vector2Int realPosition,Vector2Int positionInPartition) {
            TileItem tileItem = itemRegistry.GetTileItem(id);
            
            if (ReferenceEquals(tileItem,null)) return;

            baseTileHardnessArray[positionInPartition.x, positionInPartition.y] = tileItem.tileOptions.hardness;
            
            if (!ReferenceEquals(tileItem.tileEntity,null)) {
                PlaceTileEntityFromLoad(
                    tileItem,
                    tileEntityOptions,
                    positionInPartition,
                    tileEntities,
                    positionInPartition.x,
                    positionInPartition.y
                );
            }
            
            IWorldTileMap iWorldTileGridMap = tileGridMaps[tileItem.tileType.toTileMapType()];
            iWorldTileGridMap.placeItemTileAtLocation(
                realPosition,
                positionInPartition,
                tileItem
            );
        }

        protected virtual void PlaceTileEntityFromLoad(TileItem tileItem, string options, Vector2Int positionInPartition, ITileEntityInstance[,] tileEntityArray, int x, int y) {
            
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
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            {
                switch (layer) {
                    case TileMapLayer.Base:
                        return itemRegistry.GetTileItem(data.baseData.ids[position.x,position.y]);
                    case TileMapLayer.Background:
                        return itemRegistry.GetTileItem(data.backgroundData.ids[position.x,position.y]);
                    default:
                        Debug.LogError("TileChunkPartition attempted to return tileitem from invalid layer " + layer.ToString());
                        return null;
                }
            }
        }

        public override void SetTile(Vector2Int tilePosition, TileMapLayer layer, TileItem tileItem)
        {
            SeralizedWorldData tileData = (SeralizedWorldData) GetData();
            string id = tileItem?.id;
            switch (layer) {
                case TileMapLayer.Base:
                    tileData.baseData.ids[tilePosition.x,tilePosition.y] = id;
                    BaseTileData baseTileData = new BaseTileData(0,0,false);
                    if (tileItem is { tile: IRestrictedTile restrictedTile }) {
                        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        baseTileData.state = restrictedTile.getStateAtPosition(mousePosition,MousePositionFactory.getVerticalMousePosition(mousePosition),MousePositionFactory.getHorizontalMousePosition(mousePosition));
                    }
                    tileData.baseData.sTileOptions[tilePosition.x, tilePosition.y] = baseTileData;
                    if (!ReferenceEquals(tileItem, null))
                    {
                        baseTileHardnessArray[tilePosition.x, tilePosition.y] = tileItem.tileOptions.hardness;
                    }
                    
                    
                    break;
                case TileMapLayer.Background:
                    tileData.backgroundData.ids[tilePosition.x,tilePosition.y] = id;
                    return;
                case TileMapLayer.Fluid:
                    tileData.fluidData.ids[tilePosition.x,tilePosition.y] = id;
                    return;
            }
        }

        public override PartitionFluidData GetFluidData()
        {
            SeralizedWorldData serializedTileData = (SeralizedWorldData) GetData();
            return new PartitionFluidData(serializedTileData.fluidData.ids,serializedTileData.baseData.ids,serializedTileData.fluidData.fill);
        }

        public override bool GetFarLoaded()
        {
            return farLoaded;
        }

        public override void LoadFarLoadTileEntities()
        {
            if (tileEntities == null || farLoaded) {
                return;
            }
            farLoaded = true;
            SeralizedWorldData data = (SeralizedWorldData) GetData();
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
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
                    TileItem tileItem = itemRegistry.GetTileItem(id);
                    if (tileItem == null) {
                        continue;
                    }
                    TileEntityObject tileEntity = tileItem.tileEntity;
                    if (tileEntity == null || !tileEntity.ExtraLoadRange) {
                        continue;
                    }
                    string tileEntityData = data.baseData.sTileEntityOptions[x,y];
                    Vector2Int position = this.position * Global.ChunkPartitionSize + new Vector2Int(x,y);
                    tileEntities[x,y] = TileEntityHelper.placeTileEntity(tileItem,position,parent,true,true,tileEntityData);
                }
            }
        }

        public override void UnloadFarLoadTileEntities()
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
                    TileEntityObject tileEntity = tileEntityInstance.GetTileEntity();
                    if (tileEntity.ExtraLoadRange) {
                        unloadTileEntity(tileEntities,x,y);
                    }
                }
            }
        }
    }
}