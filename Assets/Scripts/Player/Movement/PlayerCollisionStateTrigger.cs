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
            GroundContact = Player.CollisionState.OnGround,
            HeadInFluid = Player.CollisionState.HeadInFluid,
            FeetInFluid = Player.CollisionState.FeetInFluid,
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
                case TriggerableCollisionState.HeadInFluid:
                case TriggerableCollisionState.FeetInFluid:
                    return FLUID_TAG;
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
            switch (CollisionState)
            {
                case TriggerableCollisionState.HeadInFluid:
                case TriggerableCollisionState.FeetInFluid:
                    FluidTileMap fluidTileMap = other.GetComponent<FluidTileMap>();
                    if (!fluidTileMap)
                    {
                        fluidTileMap = other.GetComponentInParent<FluidTileMap>();
                    }
                    if (!fluidTileMap) return;
                    SpriteRenderer spriteRenderer = playerRobot.GetComponent<SpriteRenderer>();
                    Vector2 playerPosition = (Vector2)playerRobot.transform.position + Vector2.down * spriteRenderer.bounds.extents.y;
                    
                    FluidTileItem leftFluid = GetTileItem(playerPosition);
                    if (leftFluid)
                    {
                        playerRobot.AddFluidCollisionData((CollisionState)CollisionState, leftFluid);
                        return;
                    }
                    
                    FluidTileItem GetTileItem(Vector2 position)
                    {
                        Vector2 collisionPoint = other.ClosestPoint(position);
                        Vector2Int cellPosition = Global.GetCellPositionFromWorld(collisionPoint);
                        FluidTileItem fluidTileItem = fluidTileMap.GetFluidTile(cellPosition);
                        fluidTileMap.Disrupt(position,cellPosition,fluidTileItem);
                        return fluidTileItem;
                    }
                    
                    break;
            }
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(tagLayer)) return;
            playerRobot.RemoveCollisionState((CollisionState)CollisionState);
            
        }
    }
}
