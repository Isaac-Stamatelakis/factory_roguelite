using System;
using Chunks;
using Chunks.Systems;
using Dimensions;
using Fluids;
using Items;
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
            GroundContact = Player.CollisionState.OnGround
        }

        private const string GROUND_TAG = "Ground";
        private const string FLUID_TAG = "Fluid";
        private string GetTag(TriggerableCollisionState state)
        {
            switch (state)
            {
                case TriggerableCollisionState.HeadContact:
                case TriggerableCollisionState.LeftWall:
                case TriggerableCollisionState.RightWall:
                case TriggerableCollisionState.GroundContact:
                    return GROUND_TAG;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        [SerializeField] private TriggerableCollisionState CollisionState;
        private PlayerRobot playerRobot;
        private string tagLayer;
        
        public void Start()
        {
            playerRobot = transform.parent.GetComponentInParent<PlayerRobot>();
            
            if (!playerRobot) Debug.LogError($"Trigger has no player robot {playerRobot}");
            tagLayer = GetTag(CollisionState);
        }
        public void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(tagLayer)) return;
            playerRobot.AddCollisionState((CollisionState)CollisionState);
        }

        public void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag(tagLayer))
            {
                playerRobot.RemoveCollisionState((CollisionState)CollisionState);
                return;
            }
            playerRobot.AddCollisionState((CollisionState)CollisionState);
        }


        public void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(tagLayer)) return;
            playerRobot.RemoveCollisionState((CollisionState)CollisionState);
        }
    }
}
