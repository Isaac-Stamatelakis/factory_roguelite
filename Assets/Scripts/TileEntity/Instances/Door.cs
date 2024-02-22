using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMapModule;
using TileMapModule.Layer;
using Tiles;
using ChunkModule.PartitionModule;
using TileMapModule.Type;

namespace TileEntityModule.Instances {

    [CreateAssetMenu(fileName = "New Door", menuName = "Tile Entity/Door")]
    public class Door : TileEntity, IClickableTileEntity
    {
        public void onClick()
        {
            if (tile is not ITypeSwitchType switchType) {
                Debug.LogError("Door Tile Entity belongs to non switch tile");
                return;
            }
            if (tile is not IRestrictedTile doortile) {
                Debug.LogError("Door Tile Entity belongs to non door tile");
                return;
            }
            IChunkPartition chunkPartition = getPartition();
            Vector2Int positionInPartition = getPositionInPartition();
            TileOptions tileOptions = chunkPartition.getTileOptions(positionInPartition);
            int state = tileOptions.SerializedTileOptions.state;

            // Remove from old tilemap
            TileMapType tileMapType = switchType.getStateType(state);
            TileMapModule.ITileMap tilemap = chunk.getTileMap(tileMapType);
            tilemap.removeForSwitch(getCellPosition());

            // Switch to open/closed
            state += 2; 
            state = state % doortile.getStateAmount();
            SerializedTileOptions serializedTileOptions = tileOptions.SerializedTileOptions;
            serializedTileOptions.state = state;
            tileOptions.SerializedTileOptions = serializedTileOptions;

            // Set tile on new tilemap
            TileMapType newType = switchType.getStateType(state);
            TileMapModule.ITileMap newMap = chunk.getTileMap(newType);
            newMap.placeTileAtLocation(getCellPosition(),doortile.getTileAtState(state));
        }
    }

}