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
        private float colliderBounds;
        public void Start()
        {
            playerRobot = transform.parent.GetComponentInParent<PlayerRobot>();
            Collider2D platformCollider = GetComponent<Collider2D>();
            colliderBounds = platformCollider.bounds.extents.y;
        }
        
        public void OnTriggerStay2D(Collider2D other)
        {
            Debug.Log(OnPlatform(other));
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
            return collisionPoint.y < transform.position.y-colliderBounds;
        }
        

        public void OnTriggerExit2D(Collider2D other)
        {
            playerRobot.RemoveCollisionState(CollisionState.OnPlatform);
            
        }
    }
}
