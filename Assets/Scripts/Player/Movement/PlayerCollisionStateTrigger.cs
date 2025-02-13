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
    public class PlayerCollisionStateTrigger : MonoBehaviour
    {
        public CollisionState CollisionState;
        private PlayerRobot playerRobot;
        
        public void Start()
        {
            playerRobot = transform.parent.GetComponentInParent<PlayerRobot>();
            if (CollisionState == CollisionState.OnSlope) Debug.LogError($"Invalid Collision State {CollisionState}");
            if (!playerRobot) Debug.LogError($"Trigger has no player robot {playerRobot}");
        }
        public void OnTriggerEnter2D(Collider2D other)
        {
            playerRobot.AddCollisionState(CollisionState);
            
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            playerRobot.RemoveCollisionState(CollisionState);
            
        }
    }
}
