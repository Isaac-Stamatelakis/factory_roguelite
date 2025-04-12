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
        private TileItem cachedTileItem;
        public void Start()
        {
            playerRobot = transform.parent.GetComponentInParent<PlayerRobot>();
        }
        
        public void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag("Ground")) return;
            bool? onslope = OnSlope(other);
            bool cached = onslope == null;
            if (cached) return;
            
            if (onslope.Value)
            {
                playerRobot.AddCollisionState(CollisionState.OnSlope);
            }
            else
            {
                playerRobot.RemoveCollisionState(CollisionState.OnSlope);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns>True if on slope, false if not on slope, null if tile item is cached</returns>
        private bool? OnSlope(Collider2D other)
        {
            Vector2Int cellPosition = Global.GetCellPositionFromWorld(other.ClosestPoint(transform.position));
            ILoadedChunkSystem system = DimensionManager.Instance.GetPlayerSystem();
            var (partition, positionInPartition) = system.GetPartitionAndPositionAtCellPosition(cellPosition);
            TileItem tileItem = partition?.GetTileItem(positionInPartition,TileMapLayer.Base);
            if (ReferenceEquals(tileItem, cachedTileItem)) return null;
            cachedTileItem = tileItem;
            if (tileItem?.tile is not HammerTile hammerTile) return false;
            BaseTileData baseTileData = partition.GetBaseData(positionInPartition);
            return baseTileData.state > 1;
            // This might not be needed
            //if (hammerTile is not NatureTile natureTile) return baseTileData.state > 1;

            //int natureSlantLength = natureTile.natureSlants.Length;
            //return baseTileData.state > 1 && baseTileData.state < 4 + natureSlantLength;
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            cachedTileItem = null;
            playerRobot.RemoveCollisionState(CollisionState.OnSlope);
            
        }
    }
}
