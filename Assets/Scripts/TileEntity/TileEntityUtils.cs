using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Chunks.Partitions;
using Tiles;
using TileMaps.Type;
using UnityEngine.Tilemaps;
using Chunks.Systems;
using Conduits.Ports;
using Entities;
using Item.Slot;
using Newtonsoft.Json;
using TileEntity.AssetManagement;
using TileEntity.Instances.CompactMachines;
using TileEntity.MultiBlock;
using TileMaps;
using TileMaps.Layer;
using UI;

namespace TileEntity {
    public static class TileEntityUtils {
        
        public static T GetConduitInteractable<T>(ITileEntityInstance tileEntityInstance, ConduitType conduitType) where T : IConduitInteractable
        {
            if (tileEntityInstance is T conduitInteractable) return conduitInteractable;
            if (tileEntityInstance is IConduitPortTileEntityAggregator portTileEntityAggregator)
            {
                if (portTileEntityAggregator.GetConduitInteractable(conduitType) is T aggregatorInteractable) return aggregatorInteractable;
            }
            return default;
        }
        public static ITileEntityInstance placeTileEntity(TileItem tileItem, Vector2Int positionInChunk, IChunk chunk, bool load, bool unserialize = false, 
            string data = null, bool assembleMultiblocks = false, bool loadAssets = true) {
            ITileEntityInstance tileEntityInstance = tileItem.tileEntity.CreateInstance(positionInChunk, tileItem, chunk);
            
            if (loadAssets && tileItem.tileEntity is IUITileEntity uiTileEntity) {
                TileEntityAssetRegistry.Instance.LoadAsset(TileEntityAssetType.UI,tileItem.tileEntity,uiTileEntity.GetUIAssetReference());
            }
            
            if (loadAssets && tileItem.tileEntity is IAssetTileEntity assetTileEntity) {
                TileEntityAssetRegistry.Instance.LoadAsset(TileEntityAssetType.UI,tileItem.tileEntity,assetTileEntity.GetAssetReference());
            }
            if (load && tileEntityInstance is ILoadableTileEntity loadableTileEntity) {
                loadableTileEntity.Load();
            }

            if (tileEntityInstance is ICompactMachineInteractable compactMachineInteractable)
            {
                var system = chunk.GetChunkSystem();
                if (system is ICompactMachineClosedChunkSystem compactMachineClosedChunkSystem)
                {
                    compactMachineInteractable.SyncToCompactMachine(compactMachineClosedChunkSystem.GetCompactMachine());
                }
            }

            if (assembleMultiblocks && tileEntityInstance is IMultiBlockTileEntity multiBlockTileEntity)
            {
                multiBlockTileEntity.AssembleMultiBlock();
            }
            
            if (data == null && tileEntityInstance is IPlaceInitializable placeInitializable)
            {
                placeInitializable.PlaceInitialize();
            } else if (data != null && unserialize && tileEntityInstance is ISerializableTileEntity serializableTileEntity)
            {
                try
                {
                    serializableTileEntity.Unserialize(data);
                }
                catch (JsonSerializationException e)
                {
                    Debug.LogWarning($"Could not deserialize tile entity {tileItem.tileEntity.name} at {tileEntityInstance.GetCellPosition()}\n{e}");
                    if (tileEntityInstance is IPlaceInitializable placeInitializable1)
                    {
                        placeInitializable1.PlaceInitialize();
                    }
                }
            }

            if (load && tileEntityInstance is ICompactMachine compactMachine)
            {
                compactMachine.PlaceInitializeWithHash(data);
            }
            return tileEntityInstance;
        }

        public static void UpdateMultiBlockOnPlace(ITileEntityInstance tileEntityInstance, IMultiBlockTileAggregate multiBlockTileAggregate)
        {
            // Search adjacent tiles for connections
            List<Vector2Int> directions = new List<Vector2Int>
            {
                Vector2Int.left,
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.down,
            };

            HashSet<IMultiBlockTileEntity> adjacentMultiBlocks = new HashSet<IMultiBlockTileEntity>();
            ILoadedChunkSystem system = tileEntityInstance.GetChunk().GetChunkSystem();
            foreach (Vector2Int direction in directions)
            {
                var (partition, positionInPartition) = system.GetPartitionAndPositionAtCellPosition(direction + tileEntityInstance.GetCellPosition());
                ITileEntityInstance adjacentTileEntity = partition.GetTileEntity(positionInPartition);
                switch (adjacentTileEntity)
                {
                    case IMultiBlockTileEntity multiBlockTileEntity:
                        adjacentMultiBlocks.Add(multiBlockTileEntity);
                        continue;
                    case IMultiBlockTileAggregate adjacentAggregate:
                    {
                        IMultiBlockTileEntity adjacentMultiBlockTileEntity = adjacentAggregate.GetAggregator();
                        if (adjacentMultiBlockTileEntity == null) continue;
                        adjacentMultiBlocks.Add(adjacentMultiBlockTileEntity);
                        break;
                    }
                }
            }
            
            foreach (IMultiBlockTileEntity adjacentMultiBlock in adjacentMultiBlocks)
            {
                RefreshMultiBlock(adjacentMultiBlock);
            }
        }

        public static void RefreshMultiBlock(IMultiBlockTileEntity multiBlockTileEntity)
        {
            ILoadableTileEntity loadableTileEntity = multiBlockTileEntity as ILoadableTileEntity;
            loadableTileEntity?.Unload();
            multiBlockTileEntity.AssembleMultiBlock();
            loadableTileEntity?.Load();
        }
        public static void spawnItemsOnBreak(List<ItemSlot> items, Vector2 worldPosition, ILoadedChunk loadedChunk) {
            foreach (ItemSlot itemSlot in items) {
                ItemEntityFactory.SpawnItemEntityFromBreak(
                    worldPosition,
                    itemSlot,
                    loadedChunk.GetEntityContainer()
                );
            }
        }
        public static void stateIterate(ITileEntityInstance tileEntity, int iterationAmount) {
            TileBase tile = tileEntity.GetTile();
            IChunk chunk = tileEntity.GetChunk();
            if (tile is not ITypeSwitchType switchType) {
                Debug.LogError("Tile Entity belongs to non switch tile");
                return;
            }
            if (tile is not IStateTile stateTile) {
                Debug.LogError("Tile Entity belongs to non state tile");
                return;
            }
            if (chunk is not ILoadedChunk loadedChunk) {
                Debug.LogError("Attempted to layer switch in unloaded chunk");
                return;
            }
            IChunkPartition chunkPartition = tileEntity.GetPartition();
            Vector2Int positionInPartition = tileEntity.GetPositionInPartition();
            
            BaseTileData baseTileData = chunkPartition.GetBaseData(positionInPartition);
            int state = baseTileData.state;
            
            // Remove from old tilemap
            TileMapType tileMapType = switchType.getStateType(state);
            Tilemap tilemap = loadedChunk.GetTileMap(tileMapType).GetTilemap();
            Vector3Int cellPosition = (Vector3Int)tileEntity.GetCellPosition();
            tilemap.SetTile(cellPosition,null);

            // Switch to open/closed
            state += iterationAmount; 
            state %= stateTile.getStateAmount();
            baseTileData.state = state;
            
            // Set tile on new tilemap
            TileMapType newType = switchType.getStateType(state);
            Tilemap newMap = loadedChunk.GetTileMap(newType).GetTilemap();
            newMap.SetTile(cellPosition, stateTile.getTileAtState(state));
        }

        public static void stateSwitch(ITileEntityInstance tileEntity, int state) {
            TileBase tile = tileEntity.GetTile();
            IChunk chunk = tileEntity.GetChunk();
            if (tile is not ITypeSwitchType switchType) {
                Debug.LogError("Tile Entity belongs to non switch tile");
                return;
            }
            if (tile is not IStateTile stateTile) {
                Debug.LogError("Tile Entity belongs to non state tile");
                return;
            }
            if (chunk is not ILoadedChunk loadedChunk) {
                return;
            }
            IChunkPartition chunkPartition = tileEntity.GetPartition();
            Vector2Int positionInPartition = tileEntity.GetPositionInPartition();
            
            BaseTileData baseTileData = chunkPartition.GetBaseData(positionInPartition);
            int oldState = baseTileData.state;
            baseTileData.state = state;
            
            TileMapType tileMapType = switchType.getStateType(oldState);
            Tilemap tilemap = loadedChunk.GetTileMap(tileMapType).GetTilemap();
            Vector3Int cellPosition = (Vector3Int)tileEntity.GetCellPosition();
            tilemap.SetTile(cellPosition,null);
            
            // Set tile on new tilemap
            TileMapType newType = switchType.getStateType(state);
            Tilemap newMap = loadedChunk.GetTileMap(newType).GetTilemap();
            newMap.SetTile(cellPosition, stateTile.getTileAtState(state));
        }

        /// <summary>
        /// Returns the tileEntity at offset position relative to tileEntity position
        /// </summary>
        public static ITileEntityInstance getAdjacentTileEntity(ITileEntityInstance tileEntity, Vector2Int offset) {
            
            IChunk chunk = tileEntity.GetChunk();
            Vector2Int offsetCellPosition = tileEntity.GetCellPosition()+offset;
            Vector2Int chunkPosition = Global.getChunkFromCell(offsetCellPosition);
            Vector2Int partitionPosition = Global.getPartitionFromCell(offsetCellPosition)-chunkPosition*Global.PARTITIONS_PER_CHUNK; 
            IChunkPartition partition = null;
            if (chunk is ILoadedChunk loadedChunk) {
                ClosedChunkSystem closedChunkSystem = loadedChunk.GetSystem();
                ILoadedChunk adjacentChunk = closedChunkSystem.GetChunk(chunkPosition);
                if (adjacentChunk == null) {
                    Debug.LogError("Attempted to locate adjcent tile entity in null chunk");
                    return null;
                }
                partition = adjacentChunk.GetPartition(partitionPosition);
            } else if (chunk is ISoftLoadedChunk softLoadedChunk) {
                ClosedChunkSystemAssembler closedChunkSystemAssembler = softLoadedChunk.getSystem();
                SoftLoadedConduitTileChunk adjacentSoftLoadedChunk = closedChunkSystemAssembler.getChunk(chunkPosition);
                if (adjacentSoftLoadedChunk == null) {
                    Debug.LogError("Attempted to locate adjcent tile entity in null chunk");
                    return null;
                }
                partition = adjacentSoftLoadedChunk.GetPartition(partitionPosition);   
            }
            if (partition == null) {
                Debug.LogError("Attempted to locate adjcaent tile entity in null partition");
                return null;
            }
            Vector2Int positionInPartition = Global.getPositionInPartition(offsetCellPosition);
            return partition.GetTileEntity(positionInPartition);
            
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="tileEntityInstance"></param>
        /// <param name="multiBlockCast"></param>
        /// <param name="positions">Adjacent tile entity positions</param>
        public static bool SyncTileMultiBlockAggregates(ITileEntityInstance tileEntityInstance, IMultiBlockTileEntity multiBlockCast, List<Vector2Int> positions)
        {
            ILoadedChunkSystem system = tileEntityInstance.GetChunk().GetChunkSystem();
            bool alreadyConnected = false;
            foreach (Vector2Int position in positions)
            {
                var (partition, positionInPartition) = system.GetPartitionAndPositionAtCellPosition(position);
                ITileEntityInstance connectedInstance = partition.GetTileEntity(positionInPartition);
                if (connectedInstance is not IMultiBlockTileAggregate multiBlockAggregate) continue;
                IMultiBlockTileEntity multiBlockTileEntity = multiBlockAggregate.GetAggregator();
                bool connectedToOther = multiBlockTileEntity != null && !ReferenceEquals(multiBlockTileEntity, multiBlockCast);
                if (connectedToOther)
                {
                    alreadyConnected = true;
                    continue;
                }
                multiBlockAggregate.SetAggregator(multiBlockCast);
            }

            return alreadyConnected;
        }
        
        public static bool SyncTileMultiBlockAggregates<T>(ITileEntityInstance tileEntityInstance, IMultiBlockTileEntity multiBlockCast, List<T> aggregates) where T : IMultiBlockTileAggregate
        {
            bool alreadyConnected = false;
            foreach (T multiBlockAggregate in aggregates)
            {
                IMultiBlockTileEntity multiBlockTileEntity = multiBlockAggregate.GetAggregator();
                bool connectedToOther = multiBlockTileEntity != null && !ReferenceEquals(multiBlockTileEntity, multiBlockCast);
                if (connectedToOther)
                {
                    alreadyConnected = true;
                    continue;
                }
                multiBlockAggregate.SetAggregator(multiBlockCast);
            }

            return alreadyConnected;
        }

        public static List<Vector2Int> BFSTile(ITileEntityInstance tileEntityInstance, TileItem tileItem, bool includeSelf = true)
        {
            TileType tileType = tileItem.tileType;
            TileMapType tileMapType = tileType.toTileMapType();
            TileMapLayer layer = tileMapType.toLayer();
            
            IChunk chunk = tileEntityInstance.GetChunk();
            ILoadedChunkSystem iLoadedChunkSystem = chunk.GetChunkSystem();
            
            Vector2Int origin = tileEntityInstance.GetCellPosition();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            
            queue.Enqueue(origin);
            visited.Add(origin);
            Vector2Int[] directions = {
                Vector2Int.left,
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.down,
            };
            List<Vector2Int> result = new List<Vector2Int>();
            if (includeSelf) result.Add(origin);
            
            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                
                foreach (var direction in directions)
                {
                    Vector2Int neighborPosition = current + direction;
                    bool alreadyVisited = !visited.Add(neighborPosition);
                    if (alreadyVisited) continue;

                    var (partition, positionInPartition) = iLoadedChunkSystem.GetPartitionAndPositionAtCellPosition(neighborPosition);
                    TileItem neighborTileItem = partition.GetTileItem(positionInPartition, layer);
                    if (ReferenceEquals(neighborTileItem,null) || neighborTileItem.id != tileItem.id) continue;
                    queue.Enqueue(neighborPosition);
                    result.Add(neighborPosition); 
                }
            }
    
            return result;
        }

        public static List<T> BFSTileEntityComponent<T>(ITileEntityInstance tileEntityInstance, TileType tileType)
        {
            TileMapType tileMapType = tileType.toTileMapType();
            TileMapLayer layer = tileMapType.toLayer();
            
            IChunk chunk = tileEntityInstance.GetChunk();
            ILoadedChunkSystem iLoadedChunkSystem = chunk.GetChunkSystem();
            
            Vector2Int origin = tileEntityInstance.GetCellPosition();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            
            queue.Enqueue(origin);
            visited.Add(origin);
            Vector2Int[] directions = {
                Vector2Int.left,
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.down,
            };
            List<T> result = new List<T>();
            
            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                
                foreach (var direction in directions)
                {
                    Vector2Int neighborPosition = current + direction;
                    bool alreadyVisited = !visited.Add(neighborPosition);
                    if (alreadyVisited) continue;

                    var (partition, positionInPartition) = iLoadedChunkSystem.GetPartitionAndPositionAtCellPosition(neighborPosition);
                    ITileEntityInstance neighborTileEntity = partition.GetTileEntity(positionInPartition);
                    
                    if (neighborTileEntity is not T value) continue;
                    queue.Enqueue(neighborPosition);
                    result.Add(value); 
            
                }
            }
    
            return result;
        }
        
    }
}
