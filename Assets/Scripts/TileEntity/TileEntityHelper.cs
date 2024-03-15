using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule;
using ChunkModule.PartitionModule;
using Tiles;
using TileMapModule.Type;
using UnityEngine.Tilemaps;
using ChunkModule.ClosedChunkSystemModule;

namespace TileEntityModule {
    public static class TileEntityHelper {
        public static void setParentOfSpawnedObject(GameObject spawned, ILoadedChunk loadedChunk) {
            spawned.transform.SetParent(loadedChunk.getTileEntityContainer(),false);
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
            TileMapModule.ITileMap tilemap = loadedChunk.getTileMap(tileMapType);
            tilemap.removeForSwitch(tileEntity.getCellPosition());

            // Switch to open/closed
            state += iterationAmount; 
            state = state % stateTile.getStateAmount();
            SerializedTileOptions serializedTileOptions = tileOptions.SerializedTileOptions;
            serializedTileOptions.state = state;
            tileOptions.SerializedTileOptions = serializedTileOptions;

            // Set tile on new tilemap
            TileMapType newType = switchType.getStateType(state);
            TileMapModule.ITileMap newMap = loadedChunk.getTileMap(newType);
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
                Debug.LogError("Attempted to layer switch in unloaded chunk");
                return;
            }
            IChunkPartition chunkPartition = tileEntity.getPartition();
            Vector2Int positionInPartition = tileEntity.getPositionInPartition();
            TileOptions tileOptions = chunkPartition.getTileOptions(positionInPartition);

            TileMapType tileMapType = switchType.getStateType(state);
            TileMapModule.ITileMap tilemap = loadedChunk.getTileMap(tileMapType);
            tilemap.removeForSwitch(tileEntity.getCellPosition());

           
            SerializedTileOptions serializedTileOptions = tileOptions.SerializedTileOptions;
            serializedTileOptions.state = state;
            tileOptions.SerializedTileOptions = serializedTileOptions;

            // Set tile on new tilemap
            TileMapType newType = switchType.getStateType(state);
            TileMapModule.ITileMap newMap = loadedChunk.getTileMap(newType);
            newMap.placeTileAtLocation(tileEntity.getCellPosition(),stateTile.getTileAtState(state));
        }

        /// <summary>
        /// Returns the tileEntity at offset position relative to tileEntity position
        /// </summary>
        public static TileEntity getAdjacentTileEntity(TileEntity tileEntity, Vector2Int offset) {
            IChunk chunk = tileEntity.getChunk();
            if (chunk is not ILoadedChunk loadedChunk) {
                Debug.LogError("Attempted to locate adjacent tile entities for unloaded chunk");
                return null;
            }
            ClosedChunkSystem closedChunkSystem = loadedChunk.getSystem();

            Vector2Int offsetCellPosition = tileEntity.getCellPosition()+offset;
            Vector2Int chunkPosition = Global.getChunkFromCell(offsetCellPosition);
            ILoadedChunk adjacentChunk = closedChunkSystem.getChunk(chunkPosition);
            if (adjacentChunk == null) {
                Debug.LogError("Attempted to locate adjacent tile entity in null chunk");
                return null;
            }

            Vector2Int partitionPosition = Global.getPartitionFromCell(offsetCellPosition)-chunkPosition*Global.PartitionsPerChunk; 
            IChunkPartition partition = adjacentChunk.getPartition(partitionPosition);
            if (partition == null) {
                Debug.LogError("Attempted to locate adjcaent tile entity in null partition");
                return null;
            }

            Vector2Int positionInPartition = Global.getPositionInPartition(offsetCellPosition);
            return partition.GetTileEntity(positionInPartition);
        }
    }
}
