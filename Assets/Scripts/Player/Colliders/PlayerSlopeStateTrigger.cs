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
    public class PlayerSlopeStateTrigger : MonoBehaviour
    {
        private PlayerRobot playerRobot;
        private Direction currentSlopeDirection;
        private Vector2 defaultSize;
        private BoxCollider2D boxCollider;
        
        private Rigidbody2D rb;
      
        public void Start()
        {
            playerRobot = transform.parent.GetComponentInParent<PlayerRobot>();
            rb = playerRobot.GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            defaultSize = boxCollider.size;
        }

        public void UpdateSize(float velocity, bool jumping)
        {
            Vector2 size = defaultSize;
            if (jumping)
            {
                boxCollider.size = size;
                return;
            }
            Vector2 bonusSize = Mathf.Abs(rb.velocity.x) * new Vector2(0.025f, 0.05f);
            boxCollider.size = size + bonusSize;
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            SlopeUpdate(other);
        }

        public void OnTriggerStay2D(Collider2D other)
        {
            SlopeUpdate(other);
        }

        private void SlopeUpdate(Collider2D other)
        {
            if (!other.CompareTag("Ground")) return;
            bool onSlope = OnSlope(other);
            
            bool collisionStateActive = playerRobot.CollisionStateActive(CollisionState.OnSlope);
            
            if (onSlope && !collisionStateActive)
            {
                playerRobot.AddCollisionState(CollisionState.OnSlope);
                playerRobot.OnSlopeAddUpdate(currentSlopeDirection);
            } else if (!onSlope && collisionStateActive)
            {
                playerRobot.RemoveCollisionState(CollisionState.OnSlope);
            } else if (onSlope)
            {
                playerRobot.OnSlopeStay(currentSlopeDirection);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns>True if on slope, false if not on slope, null if tile item is cached</returns>
        private bool OnSlope(Collider2D other)
        {
            Vector2Int cellPosition = Global.WorldToCell(other.ClosestPoint(transform.position));
            ILoadedChunkSystem system = DimensionManager.Instance.GetPlayerSystem();
            var (partition, positionInPartition) = system.GetPartitionAndPositionAtCellPosition(cellPosition);
            TileItem tileItem = partition?.GetTileItem(positionInPartition,TileMapLayer.Base);
            if (tileItem?.tile is not HammerTile hammerTile) return false;
            BaseTileData baseTileData = partition.GetBaseData(positionInPartition);
            HammerTileState? hammerTileState = hammerTile.GetHammerTileState(baseTileData.state);
            currentSlopeDirection = baseTileData.rotation == 0 ? Direction.Left : Direction.Right;
            return hammerTileState is not HammerTileState.Solid and not HammerTileState.Slab;
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            playerRobot.RemoveCollisionState(CollisionState.OnSlope);
        }
    }
}
