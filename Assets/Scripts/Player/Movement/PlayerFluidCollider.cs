using System;
using Fluids;
using Items;
using Unity.VisualScripting;
using UnityEngine;

namespace Player.Movement
{
    public class PlayerFluidCollider : MonoBehaviour
    {
        private PlayerRobot playerRobot;
        private SpriteRenderer playerSpriteRenderer;
        private FluidTileMap fluidTileMap;

        public void Start()
        {
            playerRobot = transform.parent.GetComponentInParent<PlayerRobot>();
            playerSpriteRenderer = playerRobot.GetComponent<SpriteRenderer>();
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            fluidTileMap = other.GetComponent<FluidTileMap>();
            if (!fluidTileMap)
            {
                fluidTileMap = other.GetComponentInParent<FluidTileMap>();
            }
            if (!fluidTileMap) return;
            Vector2 playerPosition = (Vector2)playerRobot.transform.position + Vector2.down * playerSpriteRenderer.bounds.extents.y;
            Vector2 collisionPoint = other.ClosestPoint(playerPosition);
            Vector2Int cellPosition = Global.GetCellPositionFromWorld(collisionPoint);
            TryCollideFluid(cellPosition, collisionPoint);
        }

        public void OnTriggerStay2D(Collider2D other)
        {
            if (!fluidTileMap) return;

            if (playerRobot.InFluid())
            {
                float ran = UnityEngine.Random.value;
                if (ran > 0.1f) return;
                if (!playerRobot.IsMoving()) return;
                fluidTileMap.PlayerParticles(playerRobot.transform.position,FluidTileMap.FluidParticleType.Standard);
                return;
            }
            
            Vector2 playerPosition = (Vector2)playerRobot.transform.position + Vector2.down * playerSpriteRenderer.bounds.extents.y;
            Vector2 collisionPoint = other.ClosestPoint(playerPosition);
            Vector2Int cellPosition = Global.GetCellPositionFromWorld(collisionPoint);
            TryCollideFluid(cellPosition, collisionPoint);
        }
        
        private void TryCollideFluid(Vector2Int position, Vector2 collisionPoint)
        {
            FluidTileItem fluidTileItem = fluidTileMap.GetFluidTile(position);
            if (!fluidTileItem) return;
            
            fluidTileMap.Disrupt(collisionPoint,position,fluidTileItem);
            playerRobot.AddCollisionState(CollisionState.InFluid);
            playerRobot.AddFluidCollisionData(fluidTileItem);
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            fluidTileMap = null;
            playerRobot.RemoveCollisionState(CollisionState.InFluid);
            
        }
    }
}
