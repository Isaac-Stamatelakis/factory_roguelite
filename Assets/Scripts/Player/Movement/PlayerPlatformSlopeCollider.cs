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
        private Rigidbody2D rb;
        public void Start()
        {
            playerRobot = GetComponentInParent<PlayerRobot>();
            rb = playerRobot.GetComponent<Rigidbody2D>();
        }
        
        public void OnTriggerStay2D(Collider2D other)
        {
            Vector2 collisionPoint = other.ClosestPoint(transform.position);
            if (collisionPoint.y > transform.position.y)
            {
                playerRobot.RemoveCollisionState(CollisionState.OnPlatformSlope);
                return;
            }

            if (!playerRobot.CollisionStateActive(CollisionState.OnPlatformSlope) && playerRobot.IsGrounded())
            {
                Tilemap slopeTilemap = other.GetComponent<Tilemap>();
                Vector3Int cellPosition = slopeTilemap.WorldToCell(collisionPoint);
                cellPosition.z = 0;
                Matrix4x4 matrix4 = slopeTilemap.GetTransformMatrix(cellPosition);
                //collisionPoint.x > transform.position.x
                if (matrix4.rotation == Quaternion.identity)
                {
                    if (rb.velocity.x > 0)
                    {
                        playerRobot.RemoveCollisionState(CollisionState.OnPlatformSlope);
                        return;
                    }
                }
                else
                {
                    if (rb.velocity.x < 0)
                    {
                        playerRobot.RemoveCollisionState(CollisionState.OnPlatformSlope);
                        return;
                    }
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
