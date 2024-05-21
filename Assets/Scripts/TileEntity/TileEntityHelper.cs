using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Chunks.Partitions;
using Tiles;
using TileMaps.Type;
using UnityEngine.Tilemaps;
using Chunks.ClosedChunkSystemModule;
using Entities;

namespace TileEntityModule {
    public static class TileEntityHelper {
        public static void setParentOfSpawnedObject(GameObject spawned, ILoadedChunk loadedChunk) {
            spawned.transform.SetParent(loadedChunk.getTileEntityContainer(),false);
        }
        public static void spawnItemsOnBreak(List<ItemSlot> items, Vector2 worldPosition, ILoadedChunk loadedChunk, ClosedChunkSystem closedChunkSystem) {
            Vector2 offsetPosition = worldPosition - closedChunkSystem.getWorldDimOffset();
            Debug.Log(offsetPosition);
            foreach (ItemSlot itemSlot in items) {
                ItemEntityHelper.spawnItemEntityFromBreak(
                    offsetPosition,
                    itemSlot,
                    loadedChunk.getEntityContainer()
                );
            }
        }
        public static void stateIterate(ITileEntity tileEntity, int iterationAmount) {
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
            TileOptions tileOptions = chunkPartition.getTileOptions(positionInPartition);
            int state = tileOptions.SerializedTileOptions.state;

            // Remove from old tilemap
            TileMapType tileMapType = switchType.getStateType(state);
            TileMaps.ITileMap tilemap = loadedChunk.getTileMap(tileMapType);
            tilemap.removeForSwitch(tileEntity.getCellPosition());

            // Switch to open/closed
            state += iterationAmount; 
            state = state % stateTile.getStateAmount();
            SerializedTileOptions serializedTileOptions = tileOptions.SerializedTileOptions;
            serializedTileOptions.state = state;
            tileOptions.SerializedTileOptions = serializedTileOptions;

            // Set tile on new tilemap
            TileMapType newType = switchType.getStateType(state);
            TileMaps.ITileMap newMap = loadedChunk.getTileMap(newType);
            newMap.placeTileAtLocation(tileEntity.getCellPosition(),stateTile.getTileAtState(state));
        }

        public static void stateSwitch(ITileEntity tileEntity, int state) {
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
            TileOptions tileOptions = chunkPartition.getTileOptions(positionInPartition);
            if (tileOptions == null) {
                return;
            }
            SerializedTileOptions serializedTileOptions = tileOptions.SerializedTileOptions;
            int oldState = serializedTileOptions.state;
            serializedTileOptions.state = state;
            tileOptions.SerializedTileOptions = serializedTileOptions;
            TileMapType tileMapType = switchType.getStateType(oldState);
            TileMaps.ITileMap tilemap = loadedChunk.getTileMap(tileMapType);
            tilemap.removeForSwitch(tileEntity.getCellPosition());
            // Set tile on new tilemap
            TileMapType newType = switchType.getStateType(state);
            TileMaps.ITileMap newMap = loadedChunk.getTileMap(newType);
            newMap.placeTileAtLocation(tileEntity.getCellPosition(),stateTile.getTileAtState(state));
        }

        /// <summary>
        /// Returns the tileEntity at offset position relative to tileEntity position
        /// </summary>
        public static TileEntity getAdjacentTileEntity(TileEntity tileEntity, Vector2Int offset) {
            
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

        public static void dfsTileEntity<T>(TileEntity tileEntity, HashSet<T> visited) {
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

        public static void dfsTileEntity<T>(TileEntity tileEntity, List<T> visited) {
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
    }
}
