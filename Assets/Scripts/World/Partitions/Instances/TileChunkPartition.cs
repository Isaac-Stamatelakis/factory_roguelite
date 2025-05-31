using System;
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
using Entities.Mobs;
using Newtonsoft.Json;
using Player;
using Tiles.Fluid.Simulation;

namespace Chunks.Partitions {
public class TileChunkPartition<T> : ChunkPartition<SeralizedWorldData> where T : SeralizedWorldData
    {
        public TileChunkPartition(SeralizedWorldData data, UnityEngine.Vector2Int position, IChunk parent) : base(data, position, parent)
        {

        }
        public override IEnumerator Load(Dictionary<TileMapType, IWorldTileMap> tileGridMaps)
        {
            tileEntities ??= new Dictionary<Vector2Int, ITileEntityInstance>();
            yield return base.Load(tileGridMaps);
            
            const int ENTITY_LOAD_PER_UPDATE = 5;
            if (parent is not ILoadedChunk loadedChunk) yield break;
            Transform entityContainer = loadedChunk.GetEntityContainer();
            int loads = 0;
            for (int i = data.entityData.Count - 1; i >= 0; i--)
            {
                SeralizedEntityData entityData = data.entityData[i];
                Vector2 entityPosition = new Vector2(entityData.x, entityData.y);
                switch (entityData.type)
                {
                    case EntityType.Item:
                        ItemEntity.SpawnFromData(entityPosition,entityData.data,entityContainer);
                        break;
                    case EntityType.Mob:
                        if (entityData.data == null) break;
                        SerializedMobEntityData mobEntityData = JsonConvert.DeserializeObject<SerializedMobEntityData>(entityData.data);
                        yield return EntityRegistry.Instance.SpawnEntityCoroutine(mobEntityData,entityPosition,entityContainer);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                data.entityData.RemoveAt(i);
                loads++;
                if (loads >= ENTITY_LOAD_PER_UPDATE)
                {
                    if (!loaded) yield break;
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

        protected bool UnloadTileEntity(int x, int y) {
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
        }

        private static void PlaceBackground(string id, ItemRegistry itemRegistry, Dictionary<TileMapType, IWorldTileMap> tileGridMaps,Vector2Int realPosition,Vector2Int positionInPartition) {
            TileItem tileItem = itemRegistry.GetTileItem(id);
            if (ReferenceEquals(tileItem,null)) {
                return;
            }
            IWorldTileMap iWorldTileGridMap = tileGridMaps[tileItem.tileType.ToTileMapType()];
            iWorldTileGridMap.PlaceItemTileAtLocation(
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
            iWorldTileGridMap.PlaceItemTileAtLocation(
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
            IWorldTileMap iWorldTileGridMap = tileGridMaps[tileType.ToTileMapType()];
            iWorldTileGridMap.PlaceItemTileAtLocation(
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

        private void PlaceTileEntityFromLoad(TileItem tileItem, string options, Vector2Int positionInPartition)
        {
            tileEntities.TryGetValue(positionInPartition, out ITileEntityInstance softLoaded);
            if (softLoaded != null) {
                if (softLoaded is ILoadableTileEntity loadableTileEntity)
                {
                    loadableTileEntity.Load();
                }
                return;
            }
            Vector2Int chunkPosition = this.position * Global.CHUNK_PARTITION_SIZE + positionInPartition;
            tileEntities[positionInPartition] = TileEntityUtils.placeTileEntity(tileItem,chunkPosition,parent,true,true,options);
        }

        public override TileItem GetTileItem(Vector2Int positionInPartition, TileMapLayer layer)
        {
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            {
                switch (layer) {
                    case TileMapLayer.Base:
                        return itemRegistry.GetTileItem(data.baseData.ids[positionInPartition.x,positionInPartition.y]);
                    case TileMapLayer.Background:
                        return itemRegistry.GetTileItem(data.backgroundData.ids[positionInPartition.x,positionInPartition.y]);
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

        public override void AddFluidDataToChunk(FluidCell[][] chunkFluidCells)
        {
            int px = position.x*Global.CHUNK_PARTITION_SIZE;
            int py = position.y*Global.CHUNK_PARTITION_SIZE;
            
            string[,] baseIds = data.baseData.ids;
            string[,] fluidIds = data.fluidData.ids;
            float[,] fill = data.fluidData.fill;
            Vector2Int realPosition = GetRealPosition();
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x++)
            {
                for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++)
                {
                    string baseId = baseIds[x,y];
                    TileItem tileItem = itemRegistry.GetTileItem(baseId);
                    
                    string fluidId = fluidIds[x,y];
                    float fillValue = fill[x,y];
                    chunkFluidCells[px + x][py + y] = GetFluidCell(tileItem, new Vector2Int(x,y), realPosition, itemRegistry.GetFluidTileItem(fluidId), fillValue,false);
                }
            }
        }

        public override FluidCell GetFluidCell(Vector2Int positionInPartition, bool displayable)
        {
            int x = positionInPartition.x;
            int y = positionInPartition.y;
            string[,] baseIds = data.baseData.ids;
            string[,] fluidIds = data.fluidData.ids;
            float[,] fill = data.fluidData.fill;
            Vector2Int realPosition = GetRealPosition();
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            string baseId = baseIds[x,y];
            TileItem tileItem = itemRegistry.GetTileItem(baseId);
                    
            string fluidId = fluidIds[x,y];
            float fillValue = fill[x,y];
            FluidTileItem fluidTileItem = ItemRegistry.GetInstance().GetFluidTileItem(fluidId);
            return GetFluidCell(tileItem, positionInPartition, realPosition, fluidTileItem, fillValue,displayable);
            
        }

        private FluidCell GetFluidCell(TileItem tileItem, Vector2Int positionInPartition, Vector2Int partitionWorldPosition, FluidTileItem fluidTileItem, float fillValue, bool displayable)
        {
            int bitMap = HammerTile.GetFlowBitMap(tileItem, GetBaseData(positionInPartition));
            const int BLOCK_FLOW = 0;
            return bitMap == BLOCK_FLOW 
                ? null 
                : new FluidCell(fluidTileItem, fillValue, bitMap, partitionWorldPosition * Global.CHUNK_PARTITION_SIZE + positionInPartition,displayable);
        }
    }
}