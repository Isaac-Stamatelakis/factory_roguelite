using System;
using System.Collections.Generic;
using Chunks.Systems;
using Dimensions;
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
        internal static (Vector2,Tilemap)? GetWorldPosition(Vector2 mouseWorldPosition, List<Tilemap> tilemaps, Vector2 startPosition, TileSearcherMode mode, float angleRange, int maxSearchDistance, LineRenderer t1 = null, LineRenderer t2 = null)
        {
            Vector2 distanceFromPlayer = startPosition - mouseWorldPosition;
            float angle = (Mathf.Atan2(distanceFromPlayer.y, distanceFromPlayer.x));
            
            float lowerAngle = Mathf.Repeat(angle - angleRange, 2 * Mathf.PI);
            float upperAngle = Mathf.Repeat(angle + angleRange, 2 * Mathf.PI);
            Vector2 lowerAnglePosition = new Vector2(Mathf.Cos(lowerAngle), Mathf.Sin(lowerAngle));
            Vector2 upperAnglePosition = new Vector2(Mathf.Cos(upperAngle), Mathf.Sin(upperAngle));

            if (!ReferenceEquals(t1, null) && !ReferenceEquals(t2,null))
            {
                t1.SetPositions(new Vector3[] { startPosition, startPosition+lowerAnglePosition * maxSearchDistance/2f });
                t2.SetPositions(new Vector3[] { startPosition, startPosition+upperAnglePosition * maxSearchDistance/2f });
            }
            foreach (Tilemap tilemap in tilemaps)
            {
                Vector2? result = SearchTileMap(startPosition,mouseWorldPosition,upperAnglePosition,lowerAnglePosition,tilemap,mode);
                if (result != null) return ((Vector2)result,tilemap);
            }

            return null;
        }

        private static Vector2? SearchTileMap(Vector2 startPosition, Vector2 mouseWorldPosition, Vector2 upperPosition, Vector2 lowerPosition, Tilemap tilemap, TileSearcherMode mode)
        {
            Vector2? shortestHit = null;
            float shortestDistance = float.PositiveInfinity;
            for (int i = 0; i < 10; i++)
            {
                float distance = ((upperPosition * i/2f) - lowerPosition * (i/2f)).magnitude;
                if (mode == TileSearcherMode.NearestToPlayer)
                {
                    shortestHit = Vector2.zero;
                    shortestDistance = float.PositiveInfinity;
                }
                for (float k = 0; k < distance; k += 0.5f)
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
                if (mode == TileSearcherMode.NearestToPlayer) return shortestHit;
            }
            return shortestHit;
        }
    }
}
