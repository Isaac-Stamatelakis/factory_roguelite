using System;
using Chunks;
using Chunks.Systems;
using Dimensions;
using TileMaps;
using TileMaps.Layer;
using Tiles;
using UnityEngine;

namespace Player.Movement
{
    public class PlayerPlatformCollisionTrigger : MonoBehaviour
    {
        private PlayerRobot playerRobot;
        private SpriteRenderer spriteRenderer;
        public void Start()
        {
            playerRobot = transform.parent.GetComponentInParent<PlayerRobot>();
            spriteRenderer = playerRobot.GetComponent<SpriteRenderer>();
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            CheckCollisionState(other);
        }

        public void OnTriggerStay2D(Collider2D other)
        {
            CheckCollisionState(other);
        }

        private void CheckCollisionState(Collider2D other)
        {
            if (!other.CompareTag("Ground")) return;
            if (OnPlatform(other))
            {
                playerRobot.AddCollisionState(CollisionState.OnPlatform);
            }
            else
            {
                playerRobot.RemoveCollisionState(CollisionState.OnPlatform);
            }
        }
        
        private bool OnPlatform(Collider2D other)
        {
            
            Vector3 collisionPoint = other.ClosestPoint(transform.position);
            return collisionPoint.y < transform.position.y && Mathf.Abs(playerRobot.transform.position.x - collisionPoint.x) < 0.08f;
        }
        

        public void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Ground")) return;
            playerRobot.RemoveCollisionState(CollisionState.OnPlatform);
            
        }
    }
}
