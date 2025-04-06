using System;
using System.Collections.Generic;
using Chunks.Systems;
using Dimensions;
using TileMaps;
using TileMaps.Type;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Player.Mouse
{
    internal enum TileSearcherMode
    {
        NearestToPlayer,
        NearestToMouse
    }
    public static class MousePositionTileMapSearcher
    {

        public static Vector2? GetNearestTileMapPosition(Vector2 mouseWorldPosition, Tilemap tilemap, int searchRange)
        {
            Vector2? nearestTileMapPosition = null;
            float closest = float.MaxValue;
            Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPosition);
            cellPosition.z = 0;
            if (!ReferenceEquals(tilemap.GetTile(cellPosition), null)) return mouseWorldPosition;
            
            for (int r = 1; r < searchRange; r++)
            {
                for (int x = -r; x <= r; x++)
                {
                    Vector3Int top = cellPosition + new Vector3Int(x, r, 0);
                    MouseNearCheckTileMap(tilemap, mouseWorldPosition, top, ref nearestTileMapPosition, ref closest);
                    Vector3Int bot = cellPosition + new Vector3Int(x, -r, 0);
                    MouseNearCheckTileMap(tilemap, mouseWorldPosition, bot, ref nearestTileMapPosition, ref closest);
                }

                for (int y = -r+1; y <= r-1; y++)
                {
                    Vector3Int left = cellPosition + new Vector3Int(r, y, 0);
                    MouseNearCheckTileMap(tilemap, mouseWorldPosition, left, ref nearestTileMapPosition, ref closest);
                    Vector3Int right = cellPosition + new Vector3Int(-r, y, 0);
                    MouseNearCheckTileMap(tilemap, mouseWorldPosition, right, ref nearestTileMapPosition, ref closest);
                }
                if (nearestTileMapPosition == null) continue;
                return nearestTileMapPosition;
            }

            return null;
        }

        private static void MouseNearCheckTileMap(Tilemap tilemap, Vector2 mouseWorldPosition, Vector3Int position, ref Vector2? nearest,
            ref float closest)
        {
            if (ReferenceEquals(tilemap.GetTile(position), null)) return;
            
            Vector2 worldPosition = tilemap.GetCellCenterWorld(position);
            Vector2 dif = worldPosition - mouseWorldPosition;
            float distance = dif.magnitude;
            if (!(distance < closest)) return;
            closest = distance;
            nearest = worldPosition;
        }
        
        private static Vector2? SearchTileMap(Vector2 startPosition, Vector2 mouseWorldPosition, Vector2 upperPosition, Vector2 lowerPosition, Tilemap tilemap, int maxSearchDistance)
        {
            for (int i = 0; i < maxSearchDistance; i++)
            {
                float distance = ((upperPosition * i/2f) - lowerPosition * (i/2f)).magnitude;
                Vector2? shortestHit = null;
                float shortestDistance = float.PositiveInfinity;
                for (float k = 0; k < distance; k += 0.1f)
                {
                    Vector2 searchPosition = startPosition + Vector2.Lerp(upperPosition*i/2f, lowerPosition*i/2f, k/distance);
                    Vector2 distanceFromMouse = searchPosition - mouseWorldPosition;
                    Vector3Int cellPosition = tilemap.WorldToCell(searchPosition);
                    cellPosition.z = 0;
                    if (ReferenceEquals(tilemap.GetTile(cellPosition), null)) continue;
                    if ((distanceFromMouse.magnitude >= shortestDistance)) continue;
                    shortestDistance = distanceFromMouse.magnitude;
                    shortestHit = searchPosition;
                }
                
                if (float.IsPositiveInfinity(shortestDistance)) continue;
                return shortestHit;
            }

            return null;
        }
    }
}
