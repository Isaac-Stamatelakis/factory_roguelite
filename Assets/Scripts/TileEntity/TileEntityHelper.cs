using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Chunks.Partitions;
using Tiles;
using TileMaps.Type;
using UnityEngine.Tilemaps;
using Chunks.Systems;
using Entities;
using Item.Slot;
using Newtonsoft.Json;
using UI;

namespace TileEntity {
    public static class TileEntityHelper {

        public static ITileEntityInstance placeTileEntity(TileItem tileItem, Vector2Int positionInChunk, IChunk chunk, bool load, bool unserialize = false, string data = null) {
            ITileEntityInstance tileEntityInstance = tileItem.tileEntity.createInstance(positionInChunk, tileItem, chunk);
            if (load) {
                if (tileEntityInstance is ILoadableTileEntity loadableTileEntity) {
                    loadableTileEntity.load();
                }
                if (tileItem.tileEntity is IManagedUITileEntity managedUITileEntity) {
                    TileEntityUIManager tileEntityUIManager = managedUITileEntity.getUIManager();
                    if (!tileEntityUIManager.Loaded && !tileEntityUIManager.Loading) {
                        tileEntityUIManager.loadUIIntoMemory();
                    }
                }
                if (tileItem.tileEntity is IAssetManagerTileEntity assetManagerTileEntity) {
                    
                }
            }
            if (data == null && tileEntityInstance is IPlaceInitializable placeInitializable)
            {
                placeInitializable.PlaceInitialize();
            } else if (data != null && unserialize && tileEntityInstance is ISerializableTileEntity serializableTileEntity)
            {
                try
                {
                    serializableTileEntity.unserialize(data);
                }
                catch (JsonSerializationException e)
                {
                    Debug.LogWarning($"Could not deserialize tile entity {tileItem.tileEntity.name} at {tileEntityInstance.getCellPosition()}\n{e}");
                    if (tileEntityInstance is IPlaceInitializable placeInitializable1)
                    {
                        placeInitializable1.PlaceInitialize();
                    }
                }
                
            }
            return tileEntityInstance;
        }
        public static void setParentOfSpawnedObject(GameObject spawned, ILoadedChunk loadedChunk) {
            spawned.transform.SetParent(loadedChunk.getTileEntityContainer(),false);
        }
        public static void spawnItemsOnBreak(List<ItemSlot> items, Vector2 worldPosition, ILoadedChunk loadedChunk, ClosedChunkSystem closedChunkSystem) {
            Vector2 offsetPosition = worldPosition - closedChunkSystem.getWorldDimOffset();
            foreach (ItemSlot itemSlot in items) {
                ItemEntityHelper.spawnItemEntityFromBreak(
                    offsetPosition,
                    itemSlot,
                    loadedChunk.getEntityContainer()
                );
            }
        }
        public static void stateIterate(ITileEntityInstance tileEntity, int iterationAmount) {
            TileBase tile = tileEntity.getTile();
            IChunk chunk = tileEntity.getChunk();
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
            IChunkPartition chunkPartition = tileEntity.getPartition();
            Vector2Int positionInPartition = tileEntity.getPositionInPartition();
            
            BaseTileData baseTileData = chunkPartition.GetBaseData(positionInPartition);
            int state = baseTileData.state;
            
            // Remove from old tilemap
            TileMapType tileMapType = switchType.getStateType(state);
            TileMaps.IWorldTileMap tilemap = loadedChunk.getTileMap(tileMapType);
            tilemap.removeForSwitch(tileEntity.getCellPosition());

            // Switch to open/closed
            state += iterationAmount; 
            state %= stateTile.getStateAmount();
            baseTileData.state = state;
            
            // Set tile on new tilemap
            TileMapType newType = switchType.getStateType(state);
            TileMaps.IWorldTileMap newMap = loadedChunk.getTileMap(newType);
            newMap.placeTileAtLocation(tileEntity.getCellPosition(),stateTile.getTileAtState(state));
        }

        public static void stateSwitch(ITileEntityInstance tileEntity, int state) {
            TileBase tile = tileEntity.getTile();
            IChunk chunk = tileEntity.getChunk();
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
            IChunkPartition chunkPartition = tileEntity.getPartition();
            Vector2Int positionInPartition = tileEntity.getPositionInPartition();
            
            BaseTileData baseTileData = chunkPartition.GetBaseData(positionInPartition);
            int oldState = baseTileData.state;
            baseTileData.state = state;
            
            TileMapType tileMapType = switchType.getStateType(oldState);
            TileMaps.IWorldTileMap tilemap = loadedChunk.getTileMap(tileMapType);
            tilemap.removeForSwitch(tileEntity.getCellPosition());
            
            // Set tile on new tilemap
            TileMapType newType = switchType.getStateType(state);
            TileMaps.IWorldTileMap newMap = loadedChunk.getTileMap(newType);
            newMap.placeTileAtLocation(tileEntity.getCellPosition(),stateTile.getTileAtState(state));
        }

        /// <summary>
        /// Returns the tileEntity at offset position relative to tileEntity position
        /// </summary>
        public static ITileEntityInstance getAdjacentTileEntity(ITileEntityInstance tileEntity, Vector2Int offset) {
            
            IChunk chunk = tileEntity.getChunk();
            Vector2Int offsetCellPosition = tileEntity.getCellPosition()+offset;
            Vector2Int chunkPosition = Global.getChunkFromCell(offsetCellPosition);
            Vector2Int partitionPosition = Global.getPartitionFromCell(offsetCellPosition)-chunkPosition*Global.PartitionsPerChunk; 
            IChunkPartition partition = null;
            if (chunk is ILoadedChunk loadedChunk) {
                ClosedChunkSystem closedChunkSystem = loadedChunk.getSystem();
                ILoadedChunk adjacentChunk = closedChunkSystem.getChunk(chunkPosition);
                if (adjacentChunk == null) {
                    Debug.LogError("Attempted to locate adjcent tile entity in null chunk");
                    return null;
                }
                partition = adjacentChunk.getPartition(partitionPosition);
            } else if (chunk is ISoftLoadedChunk softLoadedChunk) {
                SoftLoadedClosedChunkSystem softLoadedClosedChunkSystem = softLoadedChunk.getSystem();
                SoftLoadedConduitTileChunk adjacentSoftLoadedChunk = softLoadedClosedChunkSystem.getChunk(chunkPosition);
                if (adjacentSoftLoadedChunk == null) {
                    Debug.LogError("Attempted to locate adjcent tile entity in null chunk");
                    return null;
                }
                partition = adjacentSoftLoadedChunk.getPartition(partitionPosition);   
            }
            if (partition == null) {
                Debug.LogError("Attempted to locate adjcaent tile entity in null partition");
                return null;
            }
            Vector2Int positionInPartition = Global.getPositionInPartition(offsetCellPosition);
            return partition.GetTileEntity(positionInPartition);
            
        }

        public static void dfsTileEntity<T>(ITileEntityInstance tileEntity, HashSet<T> visited) {
            if (tileEntity == null || tileEntity is not T typedTileEntity) {
                return;
            }
            if (visited.Contains(typedTileEntity)) {
                return;
            }
            visited.Add(typedTileEntity);
            Vector2Int cellPosition = tileEntity.getCellPosition();
            dfsTileEntity<T>(getAdjacentTileEntity(tileEntity,Vector2Int.up),visited);
            dfsTileEntity<T>(getAdjacentTileEntity(tileEntity,Vector2Int.down),visited);
            dfsTileEntity<T>(getAdjacentTileEntity(tileEntity,Vector2Int.left),visited);
            dfsTileEntity<T>(getAdjacentTileEntity(tileEntity,Vector2Int.right),visited);
        }

        public static void dfsTileEntity<T>(ITileEntityInstance tileEntity, List<T> visited) {
            if (tileEntity == null || tileEntity is not T typedTileEntity) {
                return;
            }
            if (visited.Contains(typedTileEntity)) {
                return;
            }
            visited.Add(typedTileEntity);
            Vector2Int cellPosition = tileEntity.getCellPosition();
            dfsTileEntity<T>(getAdjacentTileEntity(tileEntity,Vector2Int.up),visited);
            dfsTileEntity<T>(getAdjacentTileEntity(tileEntity,Vector2Int.down),visited);
            dfsTileEntity<T>(getAdjacentTileEntity(tileEntity,Vector2Int.left),visited);
            dfsTileEntity<T>(getAdjacentTileEntity(tileEntity,Vector2Int.right),visited);
        }

        public static void DisplayTileEntityUI<T>(GameObject uiPrefab, T instance) where T : ITileEntityInstance
        {
            GameObject uiObject = GameObject.Instantiate(uiPrefab);
            uiObject.GetComponent<ITileEntityUI<T>>().DisplayTileEntityInstance(instance);
            CanvasController.Instance.DisplayObject(uiObject);
        }
    }
}
