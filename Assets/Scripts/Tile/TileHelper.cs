using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
Collection of static methods for tiles
**/
public class TileHelper 
{
    /// <summary>
    /// Returns the real world covered area of the tile
    /// </summary>
    public static FloatIntervalVector getRealCoveredArea(Vector2 realPlacePosition, Vector2 spriteSize) {
        float minX = getRealTileCenter(realPlacePosition.x) -(Mathf.Ceil(spriteSize.x/2)-1)/2;
        float maxX = getRealTileCenter(realPlacePosition.x) + (Mathf.Floor(spriteSize.x/2))/2;
        float minY = getRealTileCenter(realPlacePosition.y) -(Mathf.Ceil(spriteSize.y/2)-1)/2;
        float maxY = getRealTileCenter(realPlacePosition.y) + (Mathf.Floor(spriteSize.y/2))/2;
        return new FloatIntervalVector(
            new Interval<float>(
                getRealTileCenter(realPlacePosition.x) -(Mathf.Ceil(spriteSize.x/2)-1)/2,
                getRealTileCenter(realPlacePosition.x) + (Mathf.Floor(spriteSize.x/2))/2
            ),
            new Interval<float> (
                getRealTileCenter(realPlacePosition.y) -(Mathf.Ceil(spriteSize.y/2)-1)/2,
                getRealTileCenter(realPlacePosition.y) + (Mathf.Floor(spriteSize.y/2))/2
            )
        );
    }

    public static float getRealTileCenter(float n) {
        return Mathf.FloorToInt(2*n)/2f+0.25f;
    }
}
