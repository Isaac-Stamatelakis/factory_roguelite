using System;
using TileMaps;
using TileMaps.Type;
using Tiles.TileMap;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Player.Movement
{
    public class PlayerPlatformSlopeCollider : MonoBehaviour
    {
        private PlayerRobot playerRobot;
        public void Start()
        {
            playerRobot = GetComponentInParent<PlayerRobot>();
        }
        
        public void OnTriggerStay2D(Collider2D other)
        {
            Vector2 collisionPoint = other.ClosestPoint(transform.position);
            if (collisionPoint.y > transform.position.y)
            {
                playerRobot.RemoveCollisionState(CollisionState.OnPlatformSlope);
                return;
            }

            Tilemap platformTileMap = other.GetComponent<Tilemap>();
            Vector3Int cellPosition = platformTileMap.WorldToCell(collisionPoint);
            Matrix4x4 matrix4 = platformTileMap.GetTransformMatrix(cellPosition);
            if (matrix4.isIdentity)
            {
                if (collisionPoint.x > transform.position.x)
                {
                    playerRobot.RemoveCollisionState(CollisionState.OnPlatformSlope);
                    return;
                }
            }
            else
            {
                if (collisionPoint.x < transform.position.x)
                {
                    playerRobot.RemoveCollisionState(CollisionState.OnPlatformSlope);
                    return;
                }
            }
            playerRobot.AddCollisionState(CollisionState.OnPlatformSlope);
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            playerRobot.RemoveCollisionState(CollisionState.OnPlatformSlope);
        }
    }
}
