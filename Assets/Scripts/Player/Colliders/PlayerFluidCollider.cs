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
        private PlayerScript playerScript;
        private SpriteRenderer playerSpriteRenderer;
        private float yOffset;
        private const float PARTICLE_CHANCE = 0.1f;
        private FluidTileItem lastFluid;
        private List<Vector2> checkPositions;
        public FluidTileMap FluidTileMap { set; private get; }
        public void Start()
        {
            playerRobot = transform.GetComponent<PlayerRobot>();
            playerScript = transform.GetComponent<PlayerScript>();
            playerSpriteRenderer = playerRobot.GetComponent<SpriteRenderer>();
            yOffset = playerSpriteRenderer.bounds.extents.y-0.005f;
            enabled = false;
        }
        
        
        public void FixedUpdate()
        {
            Vector2 playerPosition = (Vector2)playerRobot.transform.position + Vector2.down * yOffset;
            Vector2Int cellPosition = (Vector2Int)FluidTileMap.GetTilemap().WorldToCell(playerPosition);
            FluidCell fluidCell = FluidTileMap.Simulator.GetFluidCell(cellPosition);
            FluidTileItem fluidTileItem = fluidCell?.FluidTileItem;
            if (!fluidTileItem)
            {
                if (!playerRobot.InFluid()) return;
                lastFluid = null;
                playerRobot.RemoveCollisionState(CollisionState.InFluid);
                return;
            }

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

            if (!inFluid)
            {
                if (!playerRobot.InFluid()) return;
                lastFluid = null;
                playerRobot.RemoveCollisionState(CollisionState.InFluid);
                return;
            }
            
            if (ReferenceEquals(lastFluid,fluidTileItem) && playerRobot.InFluid())
            {
                if (!playerRobot.IsMoving()) return;
                float ran = UnityEngine.Random.value;
                if (ran > PARTICLE_CHANCE) return;
                FluidTileMap.PlayerParticles(playerRobot.transform.position,FluidTileMap.FluidParticleType.Standard);
                return;
            }
            
            lastFluid =  fluidTileItem;
            FluidTileMap.Disrupt(playerPosition,fluidTileItem);
            playerRobot.AddCollisionState(CollisionState.InFluid);
            playerRobot.AddFluidCollisionData(fluidTileItem);
            
            
        }

        public void OnInitialized()
        {
            enabled = true;
        }
    }
}