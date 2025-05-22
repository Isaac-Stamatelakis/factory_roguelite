using System;
using TileMaps;
using TileMaps.Type;
using Tiles.TileMap;
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
        [FormerlySerializedAs("MinSize")] public float MinSizeY = 0.15f;
        [FormerlySerializedAs("MaxSize")] public float MaxSizeY = 2f;
        public float MinSizeX = 0.15f;
        public float MaxSizeX = 0.5f;
        public float MinActiveSpeed = -10;
        public float MaxActiveSpeed = -20;
        private float minY;
        public float MaxY = 0;
        public void Start()
        {
            playerRobot = GetComponentInParent<PlayerRobot>();
            rb = playerRobot.GetComponent<Rigidbody2D>();
            boxCollider2D = GetComponent<BoxCollider2D>();
            minY = transform.localPosition.y;
        }

        public void Update()
        {
            float yVelocity = rb.velocity.y;
            float t = yVelocity > MinActiveSpeed ? 0 : yVelocity / MaxActiveSpeed;
            var vector3 = transform.localPosition;
            vector3.y = Mathf.Lerp(minY, MaxY, t);
            transform.localPosition = vector3;
            var size = boxCollider2D.size;
            size.y = Mathf.Lerp(MinSizeY, MaxSizeY, t);
            size.x = !playerRobot.IsGrounded() ? MaxSizeX : MinSizeX;
            boxCollider2D.size = size;
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
