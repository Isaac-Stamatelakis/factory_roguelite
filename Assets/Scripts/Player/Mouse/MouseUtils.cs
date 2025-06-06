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
using Tiles.TileMap;
using UI;

namespace Player.Mouse
{
    public static class MouseUtils
    {
        
        public static bool HitTileLayer(ClosedChunkSystem closedChunkSystem, TileMapLayer tileMapLayer, Vector2 mousePosition, bool drop, int power, bool precise)
        {
            if (tileMapLayer == TileMapLayer.Base)
            {
                int layer = tileMapLayer.ToRaycastLayers();
                return RaycastHitBlock(mousePosition, layer, power, drop, precise);
            }
            
            foreach (TileMapType tileMapType in tileMapLayer.GetTileMapTypes()) {
                IWorldTileMap iWorldTileMap = closedChunkSystem.GetTileMap(tileMapType);
                if (iWorldTileMap is not IHitableTileMap hitableTileMap) continue;
                if (DevMode.Instance.instantBreak) {
                    return hitableTileMap.DeleteTile(mousePosition);
                }
                return hitableTileMap.HitTile(mousePosition, drop);
            }
            return false;
        }
        
        
        /// <summary>
        /// Shoots a ray at position. If ray collides with a object that has component IHitableTileMap,
        /// then deiterates the tilemap 
        /// </summary>
        /// <param name="position">Raycast position</param>
        /// <param name="layer">Unity layer for raycast</param>
        /// <returns>True if damages tilemap, false otherwise</returns>
        ///
        private static bool RaycastHitBlock(Vector2 position, int layer, int power, bool drop, bool precise)
        {
            IHitableTileMap hitableTileMap = GetHitableTileMap(position, precise,layer);
            if (hitableTileMap == null) return false;
            
            if (hitableTileMap is WorldTileMap tileGridMap) {
                Vector2Int cellPosition = Global.WorldToCell(position);
                ITileEntityInstance tileEntity = tileGridMap.GetTileEntityAtPosition(cellPosition);
                if (tileEntity is ILeftClickableTileEntity leftClickableTileEntity) {
                    leftClickableTileEntity.OnLeftClick();
                    if (!leftClickableTileEntity.CanBreak()) {
                        return false;
                    }
                }
            }
           
            if (DevMode.Instance.instantBreak) {
                hitableTileMap.DeleteTile(position);
                return true;
            }
            
            if (hitableTileMap is IConditionalHitableTileMap conditionalHitableTileMap)
            {
                if (!conditionalHitableTileMap.CanHitTile(power, position)) return false;
            }
            return hitableTileMap.HitTile(position, drop);
        }

        private static IHitableTileMap GetHitableTileMap(Vector2 position, bool precise, int layer)
        {
            if (precise)
            {
                RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, layer);
                return GetTileMapFromCollider(hit);
            }
            else
            {
                Vector2 tileCenter = TileHelper.getRealTileCenter(position);
                RaycastHit2D hit = Physics2D.BoxCast(tileCenter, Vector2.one * (Global.TILE_SIZE-0.02f), 0, Vector2.zero,Mathf.Infinity, layer);
                return GetTileMapFromCollider(hit);
            }

            IHitableTileMap GetTileMapFromCollider(RaycastHit2D hit)
            {
                if (!hit.collider) return null;
                IHitableTileMap hitableTileMap = hit.collider.gameObject.GetComponent<IHitableTileMap>();
                if (hitableTileMap != null) return hitableTileMap;
                hitableTileMap = hit.collider.gameObject.GetComponentInParent<IHitableTileMap>();
                return hitableTileMap;
            }
            
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
