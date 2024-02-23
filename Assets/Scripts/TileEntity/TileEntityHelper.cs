using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule;
using ChunkModule.PartitionModule;
using Tiles;
using TileMapModule.Type;
using UnityEngine.Tilemaps;

namespace TileEntityModule {
    public static class TileEntityHelper {
        public static void layerSwitch(ITileEntity tileEntity, int iterationAmount) {
            TileBase tile = tileEntity.getTile();
            IChunk chunk = tileEntity.getChunk();
            if (tile is not ITypeSwitchType switchType) {
                Debug.LogError("Door Tile Entity belongs to non switch tile");
                return;
            }
            if (tile is not IStateTile stateTile) {
                Debug.LogError("Door Tile Entity belongs to non state tile");
                return;
            }
            IChunkPartition chunkPartition = tileEntity.getPartition();
            Vector2Int positionInPartition = tileEntity.getPositionInPartition();
            TileOptions tileOptions = chunkPartition.getTileOptions(positionInPartition);
            int state = tileOptions.SerializedTileOptions.state;

            // Remove from old tilemap
            TileMapType tileMapType = switchType.getStateType(state);
            TileMapModule.ITileMap tilemap = chunk.getTileMap(tileMapType);
            tilemap.removeForSwitch(tileEntity.getCellPosition());

            // Switch to open/closed
            state += iterationAmount; 
            state = state % stateTile.getStateAmount();
            SerializedTileOptions serializedTileOptions = tileOptions.SerializedTileOptions;
            serializedTileOptions.state = state;
            tileOptions.SerializedTileOptions = serializedTileOptions;

            // Set tile on new tilemap
            TileMapType newType = switchType.getStateType(state);
            TileMapModule.ITileMap newMap = chunk.getTileMap(newType);
            newMap.placeTileAtLocation(tileEntity.getCellPosition(),stateTile.getTileAtState(state));
        }
    }
}
