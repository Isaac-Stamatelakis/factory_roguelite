using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FindTileAtLocation
{
    private static int maxSearchWidth = 16;
    /// <summary>
    /// Because tiles can be anywhere from 16x16 to 1x1 spaces big, 
    /// this function finds the position that the tile is placed in the tilemap.
    /// <param name = "hitPosition">The position that the raycast hit the tilemap collider at</param>
    /// <param name = "tilemap">The tilemap that is hit</param>
    /// <returns>The position of the tile that is hit at hitPosition in the tilemap </returns>
    /// </summary>
    public static Vector2Int find(Vector2Int hitPosition, Tilemap tilemap) {
        int searchWidth = 1;
        while (searchWidth < maxSearchWidth) {
            if (Global.modInt(searchWidth,2) == 0) {
                for (int x = searchWidth/2-1; x >= -searchWidth/2; x --) {
                    TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x+x,hitPosition.y-(searchWidth/2),0));
                    if (isHitTile(tileBase,searchWidth)) {
                        return new Vector2Int(hitPosition.x+x,hitPosition.y-(searchWidth/2));
                    }
                }
                for (int y = -searchWidth/2+1; y <= searchWidth/2-1; y ++) {
                    TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x-(searchWidth/2),hitPosition.y+y,0));
                    if (isHitTile(tileBase,searchWidth)) {
                        return new Vector2Int(hitPosition.x-(searchWidth/2), hitPosition.y+y);
                    }
                }
            } else {
                for (int x = -(searchWidth-1)/2; x <= (searchWidth-1)/2; x ++) {
                    TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x+x,hitPosition.y+(searchWidth-1)/2,0));
                    if (isHitTile(tileBase,searchWidth)) {
                        return new Vector2Int(hitPosition.x+x,hitPosition.y+(searchWidth-1)/2);
                    }
                }
                for (int y = (searchWidth-1)/2-1; y >= -(searchWidth-1)/2; y --) {
                    TileBase tileBase = tilemap.GetTile(new Vector3Int(hitPosition.x+(searchWidth-1)/2,hitPosition.y+y,0));
                    if (isHitTile(tileBase,searchWidth)) {
                        return new Vector2Int(hitPosition.x+(searchWidth-1)/2, hitPosition.y+y);
                    }
                }
            }
            searchWidth ++;
        }
        // Mathematically impossible to ever get here if maxSearchDist is infinity.
        // Since the biggest tile I'm probably ever gonna put in the game is 16x16, will never get here.
        Debug.LogError("FindTileAtLocation reached impossible to reach code. Something has gone very wrong!");
        return new Vector2Int(2147483647,2147483647);
    }

    /// <summary>
    /// Some logic to deal with different types of TileBases
    /// <param name = "tileBase">hit tile</param>
    /// <param name = "searchWidth"></param>
    /// <returns>True if spriteSize is greater than or equal to searchWidth </returns>
    /// </summary>
    private static bool isHitTile(TileBase tileBase, int searchWidth) {
        int spriteY = 0;
        if (tileBase is Tile) {
            spriteY = (int) Global.getSpriteSize(((Tile) tileBase).sprite).y;
        } else if (tileBase is RuleTile) {
            spriteY = (int) Global.getSpriteSize(((RuleTile) tileBase).m_DefaultSprite).y;
        } else if (tileBase is AnimatedTile) {
            spriteY = (int) Global.getSpriteSize(((AnimatedTile) tileBase).m_AnimatedSprites[0]).y;
        }
        return spriteY >= searchWidth;
    } 
}
