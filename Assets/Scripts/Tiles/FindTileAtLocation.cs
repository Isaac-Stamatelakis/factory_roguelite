using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Chunks.Systems;
using Chunks;
using Chunks.Partitions;
using TileMaps.Layer;

public static class FindTileAtLocation
{
    private const int MAX_SEARCH_WIDTH = 16;

    /// <summary>
    /// Because tiles can be anywhere from 16x16 to 1x1 spaces big, 
    /// this function finds the position that the tile is placed in the tilemap.
    /// <param name = "hitPosition">The position that the raycast hit the tilemap collider at</param>
    /// <param name = "tilemap">The tilemap that is hit</param>
    /// <returns>The position of the tile that is hit at hitPosition in the tilemap </returns>
    /// </summary>
    public static Vector2Int? Find(Vector2Int hitPosition, Tilemap tilemap) {
        int searchWidth = 1;
        while (searchWidth < MAX_SEARCH_WIDTH) {
            if (Global.ModInt(searchWidth,2) == 0) {
                for (int x = searchWidth/2-1; x >= -searchWidth/2; x --) {
                    TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x+x,hitPosition.y-(searchWidth/2),0));
                    if (IsHitTile(tileBase,searchWidth)) {
                        return new Vector2Int(hitPosition.x+x,hitPosition.y-(searchWidth/2));
                    }
                }
                for (int y = -searchWidth/2+1; y <= searchWidth/2-1; y ++) {
                    TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x-(searchWidth/2),hitPosition.y+y,0));
                    if (IsHitTile(tileBase,searchWidth)) {
                        return new Vector2Int(hitPosition.x-(searchWidth/2), hitPosition.y+y);
                    }
                }
            } else {
                for (int x = -(searchWidth-1)/2; x <= (searchWidth-1)/2; x ++) {
                    TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x+x,hitPosition.y+(searchWidth-1)/2,0));
                    if (IsHitTile(tileBase,searchWidth)) {
                        return new Vector2Int(hitPosition.x+x,hitPosition.y+(searchWidth-1)/2);
                    }
                }
                for (int y = (searchWidth-1)/2-1; y >= -(searchWidth-1)/2; y --) {
                    TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x+(searchWidth-1)/2,hitPosition.y+y,0));
                    if (IsHitTile(tileBase,searchWidth)) {
                        return new Vector2Int(hitPosition.x+(searchWidth-1)/2, hitPosition.y+y);
                    }
                }
            }
            searchWidth ++;
        }
        // Mathematically impossible to ever get here if maxSearchDist is infinity.
        // Since the biggest tile I'm probably ever gonna put in the game is 16x16, will never get here.
        Debug.LogWarning("FindTileAtLocation reached impossible to reach code. Something has gone very wrong!");
        return null;
    }

    /// <summary>
    /// Some logic to deal with different types of TileBases
    /// <param name = "tileBase">hit tile</param>
    /// <param name = "searchWidth"></param>
    /// <returns>True if spriteSize is greater than or equal to searchWidth </returns>
    /// </summary>
    private static bool IsHitTile(TileBase tileBase, int searchWidth) {
        if (!tileBase) {
            return false;
        }
        switch (tileBase)
        {
            case Tile tile:
                return IsHit(tile.sprite,searchWidth);
            case RuleTile tile:
                return IsHit(tile.m_DefaultSprite,searchWidth);
            case AnimatedTile tile:
                return IsHit(tile.m_AnimatedSprites[0],searchWidth);
            default:
                Debug.LogError("IsHitTile did not get sprite for " + tileBase.GetType());
                return false;
        }

        
    } 
    private static bool IsHit(Sprite sprite, int searchWidth)
    {
        Vector2Int spriteSize = Global.GetSpriteSize(sprite);
        return spriteSize.y >= searchWidth || spriteSize.x >= searchWidth;
    }
}
