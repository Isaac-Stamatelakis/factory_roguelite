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
        private BoxCollider2D boxCollider2D;
        public BoxCollider2D PlatformCollider;
        public float MaxY = 0;
        public Vector2 MaxSize;
        public float MinActiveSpeed = -10;
        public float MaxActiveSpeed = -20;
        public Vector2 MaxColliderSize;
        private Vector2 minColliderSize;
        public SlopeRotation SlopeDirection;
        private Vector2 minSize;
        private float minYPosition;
        public void Start()
        {
            playerRobot = GetComponentInParent<PlayerRobot>();
            rb = playerRobot.GetComponent<Rigidbody2D>();
            boxCollider2D = GetComponent<BoxCollider2D>();
            minYPosition = transform.localPosition.y;

            minSize = boxCollider2D.size;
            minColliderSize = PlatformCollider.size;
            
        }

        public void Update()
        {
            return;
            float yVelocity = rb.velocity.y;
            float t = yVelocity > MinActiveSpeed ? 0 : yVelocity / MaxActiveSpeed;
            var vector3 = transform.localPosition;
            vector3.y = Mathf.Lerp(minYPosition, MaxY, t);
            transform.localPosition = vector3;
            boxCollider2D.size = Vector2.Lerp(minSize, MaxSize, t);
            PlatformCollider.size = Vector2.Lerp(minColliderSize, MaxColliderSize, t);
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("Enter");
            UpdateCollisionState(other);
        }

        public void OnTriggerStay2D(Collider2D other)
        {
            Debug.Log("Stay");
            UpdateCollisionState(other);
        }

        public void UpdateCollisionState(Collider2D other)
        {
            Vector2 collisionPoint = other.ClosestPoint(transform.position);
            if (collisionPoint.y >= transform.position.y || collisionPoint.x >= transform.position.x)
            {
                playerRobot.RemoveCollisionState(CollisionState.OnPlatformSlope);
                return;
            }
            
            if (!playerRobot.CollisionStateActive(CollisionState.OnPlatformSlope) && playerRobot.IsGrounded())
            {
                switch (SlopeDirection)
                {
                    case SlopeRotation.Left:
                        if (rb.velocity.x > 0)
                        {
                            playerRobot.RemoveCollisionState(CollisionState.OnPlatformSlope);
                            return;
                        }
                        break;
                    case SlopeRotation.Right:
                        if (rb.velocity.x < 0)
                        {
                            playerRobot.RemoveCollisionState(CollisionState.OnPlatformSlope);
                            return;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            playerRobot.AddCollisionState(CollisionState.OnPlatformSlope);
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            Debug.Log("Exit");
            playerRobot.RemoveCollisionState(CollisionState.OnPlatformSlope);
        }
    }
}
