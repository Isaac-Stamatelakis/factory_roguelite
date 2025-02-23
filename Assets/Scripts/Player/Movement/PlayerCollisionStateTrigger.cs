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
        private enum TriggerableCollisionState
        {
            HeadContact = Player.CollisionState.HeadContact,
            LeftWall = Player.CollisionState.OnWallLeft,
            RightWall = Player.CollisionState.OnWallRight,
            GroundContact = Player.CollisionState.OnGround,
            
        }
        [SerializeField] private TriggerableCollisionState CollisionState;
        private PlayerRobot playerRobot;
        
        public void Start()
        {
            playerRobot = transform.parent.GetComponentInParent<PlayerRobot>();
            
            if (!playerRobot) Debug.LogError($"Trigger has no player robot {playerRobot}");
        }
        public void OnTriggerEnter2D(Collider2D other)
        {
            playerRobot.AddCollisionState((CollisionState)CollisionState);
            
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            playerRobot.RemoveCollisionState((CollisionState)CollisionState);
            
        }
    }
}
