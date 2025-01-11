using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Chunks;
using Chunks.Systems;
using TileMaps.Layer;
using TileMaps;
using TileMaps.Place;
using Chunks.Partitions;
using Conduit.Port.UI;
using TileMaps.Type;
using Conduits.Systems;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using Conduits;
using Dimensions;
using Items;
using TileEntity;
using Entities;
using Item.GrabbedItem;
using Item.Slot;
using Player.Tool;
using PlayerModule;
using PlayerModule.IO;
using UI;

namespace Player.Mouse
{
    public static class MouseUtils
    {
        public static void HitTileLayer(TileMapLayer tileMapLayer, Vector2 mousePosition)
        {
            if (tileMapLayer.raycastable())
            {
                int layer = tileMapLayer.toRaycastLayers();
                RaycastHitBlock(mousePosition,layer);
                return;
            }

            Transform playerTransform = PlayerManager.Instance.GetPlayer().transform;
            foreach (TileMapType tileMapType in tileMapLayer.getTileMapTypes()) {
                IWorldTileMap iWorldTileMap = DimensionManager.Instance.getPlayerSystem(playerTransform).getTileMap(tileMapType);
                if (iWorldTileMap is not IHitableTileMap hitableTileMap) continue;
                if (DevMode.Instance.instantBreak) {
                    hitableTileMap.deleteTile(mousePosition);
                } else {
                    hitableTileMap.hitTile(mousePosition);
                }
            }
        }
        
        /// <summary>
        /// Shoots a ray at position. If ray collides with a object that has component IHitableTileMap,
        /// then deiterates the tilemap 
        /// </summary>
        /// <param name="position">Raycast position</param>
        /// <param name="layer">Unity layer for raycast</param>
        /// <returns>True if damages tilemap, false otherwise</returns>
        private static bool RaycastHitBlock(Vector2 position, int layer) {
            RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, layer);
            if (ReferenceEquals(hit.collider, null)) return false;
            GameObject container = hit.collider.gameObject;
            IHitableTileMap hitableTileMap = container.GetComponent<IHitableTileMap>();
            if (hitableTileMap == null) {
                return false;
            }
            if (hitableTileMap is IWorldTileGridMap tileGridMap) {
                Vector2Int cellPosition = Global.getCellPositionFromWorld(position);
                ITileEntityInstance tileEntity = tileGridMap.getTileEntityAtPosition(cellPosition);
                if (tileEntity is ILeftClickableTileEntity leftClickableTileEntity) {
                    leftClickableTileEntity.onLeftClick();
                    if (!leftClickableTileEntity.canBreak()) {
                        return false;
                    }
                }
            }
            if (DevMode.Instance.instantBreak) {
                hitableTileMap.deleteTile(position);
            } else {
                hitableTileMap.hitTile(position);
            }
            return true;            
        }
        
        /// <summary>
        /// Shoots a ray at position with layer and returns object if hits
        /// </summary>
        /// <param name="position"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static GameObject RaycastObject(Vector2 position, int layer)
        {
            RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, layer);
            return hit.collider?.gameObject;
        }
    }
    
    
}
