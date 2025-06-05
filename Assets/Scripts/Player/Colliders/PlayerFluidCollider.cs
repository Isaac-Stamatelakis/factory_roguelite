using System;
using System.Collections.Generic;
using System.Numerics;
using Fluids;
using Items;
using Tiles.Fluid.Simulation;
using Unity.VisualScripting;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Player.Movement
{
    public interface IPlayerStartupListener
    {
        public void OnInitialized();
    }
    public class PlayerFluidCollider : MonoBehaviour, IPlayerStartupListener
    {
        private PlayerRobot playerRobot;
        private SpriteRenderer playerSpriteRenderer;
        private float yOffset;
        private const float PARTICLE_CHANCE = 0.1f;
        private FluidTileItem lastFluid;
        private List<Vector2> checkPositions;
        public FluidTileMap FluidTileMap { set; private get; }
        public void Start()
        {
            playerRobot = transform.GetComponent<PlayerRobot>();
            playerSpriteRenderer = playerRobot.GetComponent<SpriteRenderer>();
            yOffset = playerSpriteRenderer.bounds.extents.y-0.05f;
            checkPositions = new List<Vector2>
            {
                Vector2.down,
                Vector2.zero
            };
            enabled = false;
        }
        
        
        public void FixedUpdate()
        {
            foreach (Vector2 offset in checkPositions)
            {
                Vector2 playerPosition = (Vector2)playerRobot.transform.position + offset * yOffset;
                Vector2Int cellPosition = (Vector2Int)FluidTileMap.GetTilemap().WorldToCell(playerPosition);
                FluidCell fluidCell = FluidTileMap.Simulator.GetFluidCell(cellPosition);
                FluidTileItem fluidTileItem = fluidCell?.FluidTileItem;
                if (!fluidTileItem) continue;
                
                // In Interval [0,1]
                float fill = fluidCell.Liquid;
                
                float cellBottomY = cellPosition.y * Global.TILE_SIZE;
                if (fluidTileItem.fluidOptions.InvertedGravity)
                {
                    cellBottomY += Global.TILE_SIZE;
                }

                bool inFluid = fluidTileItem.fluidOptions.InvertedGravity
                    ? cellBottomY - fill * Global.TILE_SIZE < playerPosition.y
                    : cellBottomY + fill * Global.TILE_SIZE > playerPosition.y;

                if (!inFluid) continue;
                if (ReferenceEquals(lastFluid,fluidTileItem) && playerRobot.InFluid())
                {
                    // Can return here since other checks will fail. This will prioritize playing particles on players feet which is fine
                    if (!playerRobot.IsMoving()) return; 
                    float ran = UnityEngine.Random.value;
                    if (ran > PARTICLE_CHANCE) return;
                    FluidTileMap.PlayParticles(playerRobot.transform.position,FluidTileMap.FluidParticleType.Standard);
                    return;
                }
                
                lastFluid = fluidTileItem;
                FluidTileMap.Disrupt(playerPosition,fluidTileItem);
                playerRobot.AddCollisionState(CollisionState.InFluid);
                playerRobot.AddFluidCollisionData(fluidTileItem);
                return;
            }
            
            // Only gets here if not in fluid
            if (!playerRobot.InFluid()) return;
            lastFluid = null;
            playerRobot.RemoveCollisionState(CollisionState.InFluid);
        }

        public void OnInitialized()
        {
            enabled = true;
        }
    }
}