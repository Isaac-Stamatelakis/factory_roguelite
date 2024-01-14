using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacedTile
{
    private Vector2Int spriteSizeVector;
    private Vector3Int positionVector;
    public Vector3Int Position {get{return positionVector;}}

    public PlacedTile(Vector3Int positionVector, Vector2 spriteSizeVector) {
        this.positionVector = positionVector;
        this.spriteSizeVector = new Vector2Int((int) spriteSizeVector.x, (int) spriteSizeVector.y);
    }
    /// <summary>
    /// Returns the IntervalVector that the placed tile covers
    /// Odd sized sprites are easy as they are centered at the position
    /// </summary>
    public IntervalVector getCoveredArea() {
        return new IntervalVector(
            getCoveredInterval(spriteSizeVector.x,positionVector.x),
            getCoveredInterval(spriteSizeVector.y,positionVector.y)
        );
    }

    private Interval getCoveredInterval(int spriteSize, int position) {
        if (Global.modInt(spriteSize,16) == 1) { // spriteSize is odd
            return new Interval((int) (position-spriteSize/2f), (int) (position+spriteSize/2f));
        } else { // spriteSize is even
            return new Interval((int) (position-spriteSize/2+1), (int)(position+spriteSize/2));
        }
    }
    
}
