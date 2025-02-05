using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMaps;
using TileEntity;

/**
Collection of static methods for tiles
**/
public class TileHelper 
{
    /// <summary>
    /// Returns the real world covered area of the tile
    /// </summary>
    public static FloatIntervalVector getRealCoveredArea(Vector2 realPlacePosition, Vector2 spriteSize, int rotation)
    {
        const float TILE_SIZE = 0.5f;
        float minX = -(Mathf.Ceil(spriteSize.x/2)-1)/2;
        float maxX = (Mathf.Floor(spriteSize.x/2))/2;
        float minY = -(Mathf.Ceil(spriteSize.y/2)-1)/2;
        float maxY = (Mathf.Floor(spriteSize.y/2))/2;
        
        switch (rotation)
        {
            case 0:
            case 2:
                break;
            case 1: // 90 degrees
                (minX, maxX, minY, maxY) = (minY, maxY, -maxX, -minX);
                if (spriteSize.x % 2 == 0)
                {
                    minY += TILE_SIZE;
                    maxY += TILE_SIZE;
                }
                break;
            case 3: // 270 degrees
                (minX, maxX, minY, maxY) = (-maxY, -minY, minX, maxX);
                if (spriteSize.y % 2 == 0)
                {
                    minX += TILE_SIZE;
                    maxX += TILE_SIZE;
                }
                break;
            default:
                // Handle invalid rotation values
                throw new ArgumentException("Invalid rotation value.");
        }
        
        Vector2 realPlacePositionCenter = new Vector2(getRealTileCenter(realPlacePosition.x), getRealTileCenter(realPlacePosition.y));
        return new FloatIntervalVector(
            new Interval<float>(
                realPlacePositionCenter.x+minX,
                realPlacePositionCenter.x+maxX
            ),
            new Interval<float> (
                realPlacePositionCenter.y+minY,
                realPlacePositionCenter.y+maxY
            )
        );
    }

    public static float getRealTileCenter(float n) {
        return Mathf.FloorToInt(2*n)/2f+0.25f;
    }
    public static void tilePlaceTileEntityUpdate(Vector2Int position, TileItem item, WorldTileGridMap worldTileGridMap)
        {
            callTileEntityPlaceListener(worldTileGridMap.getTileEntityAtPosition(position+Vector2Int.up),item);
            callTileEntityPlaceListener(worldTileGridMap.getTileEntityAtPosition(position+Vector2Int.down),item);
            callTileEntityPlaceListener(worldTileGridMap.getTileEntityAtPosition(position+Vector2Int.left),item);
            callTileEntityPlaceListener(worldTileGridMap.getTileEntityAtPosition(position+Vector2Int.right),item);
        }

    public static void callTileEntityPlaceListener(ITileEntityInstance tileEntity, TileItem tileItem) {
        if (tileEntity == null) {
            return;
        }
        if (tileEntity is not ITileItemUpdateReciever tileUpdateReciever) {
            return;
        }
        tileUpdateReciever.tileUpdate(tileItem);
    }
}
