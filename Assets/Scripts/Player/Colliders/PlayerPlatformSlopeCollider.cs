using System;
using TileMaps;
using TileMaps.Type;
using Tiles.TileMap;
using Tiles.TileMap.Platform;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Player.Movement
{
    public class PlayerPlatformSlopeCollider : MonoBehaviour
    {
        private PlayerRobot playerRobot;
        private Rigidbody2D rb;
        public SlopeRotation SlopeDirection;
        private CollisionState collisionState;
        
        public void Start()
        {
            playerRobot = GetComponentInParent<PlayerRobot>();
            rb = playerRobot.GetComponent<Rigidbody2D>();
            switch (SlopeDirection)
            {
                case SlopeRotation.Left:
                    collisionState = CollisionState.OnLeftSlopePlatform;
                    break;
                case SlopeRotation.Right:
                    collisionState = CollisionState.OnRightSlopePlatform;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        

        public void OnTriggerEnter2D(Collider2D other)
        {
            UpdateCollisionState(other);
        }

        public void OnTriggerStay2D(Collider2D other)
        {
            UpdateCollisionState(other);
        }

        public void UpdateCollisionState(Collider2D other)
        {
            Vector2 collisionPoint = other.ClosestPoint(transform.position);
            if (collisionPoint.y >= transform.position.y)
            {
                playerRobot.RemoveCollisionState(collisionState);
                return;
            }
            
            switch (SlopeDirection)
            {
                case SlopeRotation.Left:
                    if (collisionPoint.x >= transform.position.x)
                    {
                        playerRobot.RemoveCollisionState(collisionState);
                        return;
                    }
                    break;
                case SlopeRotation.Right:
                    if (collisionPoint.x <= transform.position.x)
                    {
                        playerRobot.RemoveCollisionState(collisionState);
                        return;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (!playerRobot.CollisionStateActive(collisionState) && playerRobot.IsGrounded())
            {
                switch (SlopeDirection)
                {
                    case SlopeRotation.Left:
                        if (rb.velocity.x > 0)
                        {
                            playerRobot.RemoveCollisionState(collisionState);
                            return;
                        }
                        break;
                    case SlopeRotation.Right:
                        if (rb.velocity.x < 0)
                        {
                            playerRobot.RemoveCollisionState(collisionState);
                            return;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            playerRobot.AddCollisionState(collisionState);
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            playerRobot.RemoveCollisionState(collisionState);
        }
    }
}
