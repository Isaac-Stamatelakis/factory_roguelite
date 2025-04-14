using System.Collections;
using System.Collections.Generic;
using Chunks.Partitions;
using Items;
using UnityEngine;
using Tiles;

namespace TileMaps {
    public class BackgroundWorldTileMap : WorldTileMap
    {
        public override Vector2Int GetHitTilePosition(Vector2 position) {
            Vector3Int cellPosition = tilemap.WorldToCell(position);
            if (tilemap.GetTile(cellPosition) != null) { // Mouse was over 16x16 area of background
                return (Vector2Int)cellPosition;
            }
            float boundarySize = 0.5f/4;
            List<Vector2> directions = new List<Vector2>{
                position + new Vector2(-boundarySize,0),
                position + new Vector2(boundarySize,0),
                position + new Vector2(0,boundarySize),
                position + new Vector2(0,-boundarySize),
                position + new Vector2(-boundarySize,boundarySize),
                position + new Vector2(boundarySize,-boundarySize),
                position + new Vector2(boundarySize,boundarySize),
                position + new Vector2(-boundarySize,-boundarySize)
            };
            
            foreach (Vector2 direction in directions) {
                cellPosition = tilemap.WorldToCell(direction);
                cellPosition.z = 0;
                if (tilemap.GetTile(cellPosition) != null) {
                    return (Vector2Int)cellPosition;
                }
            }
            return (Vector2Int)cellPosition;


        }

    }

}
