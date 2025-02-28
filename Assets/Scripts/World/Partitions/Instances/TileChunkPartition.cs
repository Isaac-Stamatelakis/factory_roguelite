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
using Player;

namespace Chunks.Partitions {
public class TileChunkPartition<T> : ChunkPartition<SeralizedWorldData> where T : SeralizedWorldData
    {
        public TileChunkPartition(SeralizedWorldData data, UnityEngine.Vector2Int position, IChunk parent) : base(data, position, parent)
        {

        }
        public override IEnumerator Load(Dictionary<TileMapType, IWorldTileMap> tileGridMaps, Direction direction)
        {
            tileEntities ??= new Dictionary<Vector2Int, ITileEntityInstance>();
            yield return base.Load(tileGridMaps,direction);
            
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

        public override void Save()
        {
            if (tileEntities != null) {
                for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x++) {
                    for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++) {
                        if (!tileEntities.TryGetValue(new Vector2Int(x,y), out ITileEntityInstance tileEntityInstance)) continue;
                        if (tileEntityInstance is not ISerializableTileEntity serializableTileEntity) continue;
                        data.baseData.sTileEntityOptions[x,y] = serializableTileEntity.Serialize();
                    }
                }   
            }
        }
        
        public override IEnumerator UnloadTiles(Dictionary<TileMapType, IWorldTileMap> tileGridMaps) {
            yield return base.UnloadTiles(tileGridMaps);
            if (tileEntities == null) yield break;
            
            const int removalsPerNumeration = 5;
            int removals = 0;
                    
            for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x++) {
                for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++) {
                    if(UnloadTileEntity(x,y)) {
                        removals ++;
                    }
                    if (removals >= removalsPerNumeration) {
                        removals = 0;
                        yield return null;
                    }
                }
            }
        }

        protected virtual bool UnloadTileEntity(int x, int y) {
            Vector2Int vector = new Vector2Int(x, y);
            if (!tileEntities.TryGetValue(vector, out ITileEntityInstance tileEntityInstance)) return false;
            if (tileEntityInstance == null) {
                return false;
            }
            if (tileEntityInstance is ILoadableTileEntity loadableTileEntity) {
                loadableTileEntity.Unload();
            }

            if (tileEntityInstance is ISoftLoadableTileEntity) return false;
            if (tileEntityInstance is ISerializableTileEntity serializableTileEntity) {
                data.baseData.sTileEntityOptions[x,y] = serializableTileEntity.Serialize();
            }

            tileEntities.Remove(vector);
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
                    positionInPartition
                );
            }


            TileType tileType = GetTileType(tileItem, positionInPartition.x, positionInPartition.y);
            IWorldTileMap iWorldTileGridMap = tileGridMaps[tileType.toTileMapType()];
            iWorldTileGridMap.placeItemTileAtLocation(
                realPosition,
                positionInPartition,
                tileItem
            );
        }

        private TileType GetTileType(TileItem tileItem, int x, int y)
        {
            if (tileItem.tile is not IStateLayerTile stateLayerTile)
            {
                return tileItem.tileType;
            }
            return stateLayerTile.GetTileType(data.baseData.sTileOptions[x,y]?.state ?? 0);
        }

        protected virtual void PlaceTileEntityFromLoad(TileItem tileItem, string options, Vector2Int positionInPartition)
        {
            tileEntities.TryGetValue(positionInPartition, out ITileEntityInstance softLoaded);
            if (softLoaded != null) {
                if (softLoaded is ILoadableTileEntity loadableTileEntity)
                {
                    loadableTileEntity.Load();
                }
                return;
            }
            Vector2Int position = this.position * Global.CHUNK_PARTITION_SIZE + positionInPartition;
            tileEntities[positionInPartition] = TileEntityUtils.placeTileEntity(tileItem,position,parent,true,true,options);
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
            SeralizedWorldData tileData = GetData();
            string id = tileItem?.id;
            switch (layer) {
                case TileMapLayer.Base:
                    tileData.baseData.ids[tilePosition.x,tilePosition.y] = id;
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
      
            for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x++) {
                for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++) {
                    Vector2Int positionInPartition = new Vector2Int(x, y);
                    if (tileEntities.ContainsKey(positionInPartition)) {
                        continue;
                    }
                    string id = data.baseData.ids[x,y];
                    if (id == null) {
                        continue;
                    }
                    TileItem tileItem = itemRegistry.GetTileItem(id);
                    TileEntityObject tileEntity = tileItem?.tileEntity;
                    if (tileEntity is not { ExtraLoadRange: true }) continue;
                        
                    
                    string tileEntityData = data.baseData.sTileEntityOptions[x,y];
                    Vector2Int position = this.position * Global.CHUNK_PARTITION_SIZE + new Vector2Int(x,y);
                    tileEntities[positionInPartition] = TileEntityUtils.placeTileEntity(tileItem,position,parent,true,true,tileEntityData);
                }
            }
        }

        public override void UnloadFarLoadTileEntities()
        {
            if (tileEntities == null || !farLoaded) {
                return;
            }
            farLoaded = false;
            for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x++) {
                for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++) {
                    if (!tileEntities.TryGetValue(new Vector2Int(x,y), out ITileEntityInstance tileEntityInstance)) continue;
                    if (tileEntityInstance is null or ISoftLoadableTileEntity) {
                        continue;
                    }
                    TileEntityObject tileEntity = tileEntityInstance.GetTileEntity();
                    if (tileEntity.ExtraLoadRange) {
                        UnloadTileEntity(x,y);
                    }
                }
            }
        }
    }
}