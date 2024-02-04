using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMapHelper
{
    public static UnityEngine.Vector2Int positionToCellCords(Vector2 position, Vector2 tileMapPosition, float gridSize) {
        float xCord = position.x-tileMapPosition.x;
        float yCord = position.y-tileMapPosition.y;
        return new UnityEngine.Vector2Int(Mathf.FloorToInt(xCord / gridSize), Mathf.FloorToInt(yCord / gridSize));
    }
}
