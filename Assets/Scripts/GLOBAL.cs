using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using WorldModule.Caves;
using System.IO;
using TileEntity.Instances.CompactMachines;

public static class Global
{
    public const float TILE_SIZE = 0.5f;
    private const float PIXELS_PER_BLOCK = 16;
    public const int PARTITIONS_PER_CHUNK = 6;
    public const int CHUNK_SIZE = PARTITIONS_PER_CHUNK*CHUNK_PARTITION_SIZE;
    public const int CHUNK_PARTITION_SIZE = 4;
    public const uint SOLID_SPEED_PER_UPGRADE = 4;
    public const uint FLUID_SPEED_PER_UPGRADE = 16;
    public const int CHUNK_LOAD_RANGE = 2;
    public const uint MAX_SIZE = 999;
    public const bool ShowSystemParameter = false;
    public const int PLAYER_LAYER = 8;
    public const int BLOCK_LAYER = 512;
    
    public static float Mod(float x, float m) {
    return (x%m + m)%m;
    }

    public static int ModInt(float x, float m) {
        return (int) Mod(x,m);
    }

    public static Vector2Int GetSpriteSize(Sprite sprite)
    {
        return !sprite ? Vector2Int.zero : new Vector2Int((int) (sprite.rect.width / Global.PIXELS_PER_BLOCK), (int) (sprite.rect.height / Global.PIXELS_PER_BLOCK));
    }
    

    public static Vector2Int GetChunkFromWorld(Vector2 position) {
        return new Vector2Int(Mathf.FloorToInt(position.x/(Global.CHUNK_SIZE/2)), Mathf.FloorToInt(position.y/(Global.CHUNK_SIZE/2)));
    }
    public static Vector2Int GetChunkFromCell(Vector2Int cellPosition) {
        return new Vector2Int(Mathf.FloorToInt(((float)cellPosition.x)/(Global.CHUNK_SIZE)), Mathf.FloorToInt(((float) cellPosition.y)/(Global.CHUNK_SIZE)));
    }
    public static Vector2Int GetPartitionFromCell(Vector2Int cellPosition) {
        return new Vector2Int(Mathf.FloorToInt(((float) cellPosition.x)/(Global.CHUNK_PARTITION_SIZE)), Mathf.FloorToInt(((float)cellPosition.y)/(Global.CHUNK_PARTITION_SIZE)));
    }

  
    public static Vector2Int GetPositionInPartition(Vector2Int cellPosition)
    {
        return GetPositionInObject(cellPosition,Global.CHUNK_PARTITION_SIZE);
    }

    public static Vector2Int GetPositionInChunk(Vector2Int cellPosition)
    {
        return GetPositionInObject(cellPosition,Global.CHUNK_SIZE);
    }

    private static Vector2Int GetPositionInObject(Vector2Int cellPosition, int objectSize) {
        int x = Mathf.Abs(cellPosition.x) % objectSize;
        x = cellPosition.x < 0 && x != 0 ? objectSize - x : x;

        int y = Mathf.Abs(cellPosition.y) % objectSize;
        y = cellPosition.y < 0 && y != 0 ? objectSize - y : y;

        return new Vector2Int(x, y);
    }


    public static Vector2Int GetPartitionFromWorld(Vector2 position) {
        return new Vector2Int(Mathf.FloorToInt(position.x/(Global.CHUNK_PARTITION_SIZE/2)), Mathf.FloorToInt(position.y/(Global.CHUNK_PARTITION_SIZE/2)));
    }
    public static Vector2Int WorldToCell(Vector2 position) {
        return new Vector2Int(Mathf.FloorToInt(2*position.x), Mathf.FloorToInt(2*position.y));
    }
}
