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
        public void Start()
        {
            playerRobot = transform.parent.GetComponentInParent<PlayerRobot>();
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
            return collisionPoint.y < transform.position.y;
        }
        

        public void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Ground")) return;
            playerRobot.RemoveCollisionState(CollisionState.OnPlatform);
            
        }
    }
}
