using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using WorldModule.Caves;
using System.IO;

public static class Global
{
    public const float WORLD_SPACE_PER_TILE = 0.5f;
    private const float PIXELS_PER_BLOCK = 16;
    public const int PARTITIONS_PER_CHUNK = 6;
    public const int CHUNK_SIZE = PARTITIONS_PER_CHUNK*CHUNK_PARTITION_SIZE;
    public const int CHUNK_PARTITION_SIZE = 4;
    private static Vector2Int chunkLoadRange = new UnityEngine.Vector2Int(2,2);
    private static  int chunkPartitionExtraTileEntityLoadRange = 2;
    public const uint SOLID_SPEED_PER_UPGRADE = 4;
    public const uint FLUID_SPEED_PER_UPGRADE = 16;
    public static int ChunkPartitionExtraTileEntityLoadRange {get{return chunkPartitionExtraTileEntityLoadRange;}}
    
    public static  int ChunkLoadRangeX {get {return chunkLoadRange.x;}}
    public static int ChunkLoadRangeY {get {return chunkLoadRange.y;}}
    private static float tileBlockZ = 1;
    public static float TileBlockZ {get {return tileBlockZ;}}
    private static float entityZ = -1;
    public static float EntityZ {get {return entityZ;}}
    private static float tileObjectZ = 0.5f;
    public static float TileObjectZ {get {return tileObjectZ;}}
    public static float TileBackGroundZ { get; } = 3f;
    private static float chunkOffset = -0.25f;  
    public static float ChunkOffset {get {return chunkOffset;}}
    private static float tileItemEntityScalar = 0.5f;
    private static uint maxSize = 999;
    private static readonly bool showSystemParameter = false;
    public static uint MaxSize {get{return maxSize;}}
    public static float TileItemEntityScalar {get {return tileItemEntityScalar;}}

    private static float itemEntityLifeSpan = 300f;
    public static float ItemEntityLifeSpawn {get{return itemEntityLifeSpan;}}

    public static Cave CurrentCave { get => currentCave; set => currentCave = value; }
    public static string EditorCreationPath { get => Path.Combine("Assets",editorCreationPath); }

    public static bool ShowSystemParameter => showSystemParameter;
    private static Cave currentCave;
    public static int EXTRA_TILE_ENTITY_LOAD_RANGE = 4;
    private static readonly string editorCreationPath = "EditorCreations";
    
    public static GameObject findChild(Transform transform, string childName) {
        for (int n = 0; n < transform.childCount; n ++) {
            if (transform.GetChild(n).name == childName) {
                return transform.GetChild(n).gameObject;
            }
        }
        return null;
    }

    public static float mod(float x, float m) {
    return (x%m + m)%m;
    }

    public static int modInt(float x, float m) {
        return (int) mod(x,m);
    }

    public static Vector2Int getSpriteSize(Sprite sprite) {
        if (!sprite) {
            return Vector2Int.zero;
        }
        return new Vector2Int((int) (sprite.rect.width / Global.PIXELS_PER_BLOCK), (int) (sprite.rect.height / Global.PIXELS_PER_BLOCK));
    }

    public static Vector2Int Vector3IntToVector2Int(Vector3Int vector3Int) {
        return new Vector2Int(vector3Int.x, vector3Int.y);
    }

    public static Vector2Int getChunkFromWorld(Vector2 position) {
        return new Vector2Int(Mathf.FloorToInt(position.x/(Global.CHUNK_SIZE/2)), Mathf.FloorToInt(position.y/(Global.CHUNK_SIZE/2)));
    }
    public static Vector2Int getChunkFromCell(Vector2Int cellPosition) {
        return new Vector2Int(Mathf.FloorToInt(((float)cellPosition.x)/(Global.CHUNK_SIZE)), Mathf.FloorToInt(((float) cellPosition.y)/(Global.CHUNK_SIZE)));
    }
    public static Vector2Int getPartitionFromCell(Vector2Int cellPosition) {
        return new Vector2Int(Mathf.FloorToInt(((float) cellPosition.x)/(Global.CHUNK_PARTITION_SIZE)), Mathf.FloorToInt(((float)cellPosition.y)/(Global.CHUNK_PARTITION_SIZE)));
    }

    public static Vector2Int getPositionInPartition(Vector2Int cellPosition)
    {
        return getPositionInObject(cellPosition,Global.CHUNK_PARTITION_SIZE);
    }

    public static Vector2Int getPositionInChunk(Vector2Int cellPosition)
    {
        return getPositionInObject(cellPosition,Global.CHUNK_SIZE);
    }

    private static Vector2Int getPositionInObject(Vector2Int cellPosition, int objectSize) {
        int x = Mathf.Abs(cellPosition.x) % objectSize;
        x = cellPosition.x < 0 && x != 0 ? objectSize - x : x;

        int y = Mathf.Abs(cellPosition.y) % objectSize;
        y = cellPosition.y < 0 && y != 0 ? objectSize - y : y;

        return new Vector2Int(x, y);
    }


    public static Vector2Int getPartitionFromWorld(Vector2 position) {
        return new Vector2Int(Mathf.FloorToInt(position.x/(Global.CHUNK_PARTITION_SIZE/2)), Mathf.FloorToInt(position.y/(Global.CHUNK_PARTITION_SIZE/2)));
    }
    public static Vector2Int getCellPositionFromWorld(Vector2 position) {
        return new Vector2Int(Mathf.FloorToInt(2*position.x), Mathf.FloorToInt(2*position.y));
    }
}

public struct Dim2Bounds {
    public int XLowerBound;
    public int XUpperBound;
    public int YLowerBound;
    public int YUpperBound;
    public Dim2Bounds(int xLowerBound, int xUpperBound, int yLowerBound, int yUpperBound) {
        this.XLowerBound = xLowerBound;
        this.YLowerBound = yLowerBound;
        this.XUpperBound = xUpperBound;
        this.YUpperBound = yUpperBound;
    }

    public Vector2Int size() {
        return new Vector2Int(Mathf.Abs(XLowerBound-XUpperBound)+1,Mathf.Abs(YLowerBound-YUpperBound)+1);
    }
}