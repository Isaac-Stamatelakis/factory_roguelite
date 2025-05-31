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
            if (tilemap.GetTile(cellPosition)) { // Mouse was over 16x16 area of background
                return (Vector2Int)cellPosition;
            }
            const float BOUNDARY_SIZE = Global.TILE_SIZE/4;
            List<Vector2> directions = new List<Vector2>{
                position + new Vector2(-BOUNDARY_SIZE,0),
                position + new Vector2(BOUNDARY_SIZE,0),
                position + new Vector2(0,BOUNDARY_SIZE),
                position + new Vector2(0,-BOUNDARY_SIZE),
                position + new Vector2(-BOUNDARY_SIZE,BOUNDARY_SIZE),
                position + new Vector2(BOUNDARY_SIZE,-BOUNDARY_SIZE),
                position + new Vector2(BOUNDARY_SIZE,BOUNDARY_SIZE),
                position + new Vector2(-BOUNDARY_SIZE,-BOUNDARY_SIZE)
            };
            
            foreach (Vector2 direction in directions) {
                cellPosition = tilemap.WorldToCell(direction);
                cellPosition.z = 0;
                if (tilemap.GetTile(cellPosition)) {
                    return (Vector2Int)cellPosition;
                }
            }
            return (Vector2Int)cellPosition;


        }

    }

}
