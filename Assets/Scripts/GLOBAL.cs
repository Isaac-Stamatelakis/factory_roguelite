using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using WorldModule.Generation;

public static class Global
{
    private static int rotation = 2;
    public static int Rotation {get{return rotation;} set{rotation = value;}}
    private static float inventoryScale = 1.2f;
    public static float InventoryScale {get{return inventoryScale;}}
    private static string worldName = "world2";
    public static string WorldName {set{worldName=value;} get{return worldName;}}
    private static float pixelsPerBlock = 16;
    public static float PixelsPerBlock {get {return pixelsPerBlock;}}
    private static int partitionsPerChunk = 6;
    public static int PartitionsPerChunk {get{return partitionsPerChunk;}}
    public static int ChunkSize {get{return partitionsPerChunk*chunkPartitionSize;}}
    private static int chunkPartitionSize = 4;
    public static int ChunkPartitionSize {get{return chunkPartitionSize;}}
    private static UnityEngine.Vector2Int chunkLoadRange = new UnityEngine.Vector2Int(2,2);
    public static UnityEngine.Vector2Int ChunkPartitionLoadRange {get{return chunkPartitionLoadRange;}}
    //private static UnityEngine.Vector2Int chunkPartitionLoadRange = new UnityEngine.Vector2Int(6,5);
    private static UnityEngine.Vector2Int chunkPartitionLoadRange = new UnityEngine.Vector2Int(7,6);
    private static int chunkPartitionExtraTileEntityLoadRange = 2;
    public static int ChunkPartitionExtraTileEntityLoadRange {get{return chunkPartitionExtraTileEntityLoadRange;}}
    
    public static int ChunkLoadRangeX {get {return chunkLoadRange.x;}}
    public static int ChunkLoadRangeY {get {return chunkLoadRange.y;}}
    private static float tileBlockZ = 1;
    public static float TileBlockZ {get {return tileBlockZ;}}
    private static float entityZ = -1;
    public static float EntityZ {get {return entityZ;}}
    private static float tileObjectZ = 0.5f;
    public static float TileObjectZ {get {return tileObjectZ;}}
    private static float tileBackgroundZ = 3f;
    public static float TileBackGroundZ {get {return tileBackgroundZ;}}
    private static float energyConduitZ = 1f;
    public static float EnergyConduitZ {get {return energyConduitZ;}}
    private static float itemConduitZ = 1.5f;
    public static float ItemConduitZ {get {return itemConduitZ;}}
    private static float fluidConduitZ = 2f;
    public static float FluidConduitZ {get {return fluidConduitZ;}}
    private static float signalConduitZ = 2.5f;
    public static float SignalConduitZ {get {return signalConduitZ;}}
    private static float chunkOffset = -0.25f;  
    public static float ChunkOffset {get {return chunkOffset;}}
    private static float tileItemEntityScalar = 0.5f;
    private static int maxSize = 999;
    public static int MaxSize {get{return maxSize;}}
    public static float TileItemEntityScalar {get {return tileItemEntityScalar;}}

    private static float itemEntityLifeSpan = 300f;
    public static float ItemEntityLifeSpawn {get{return itemEntityLifeSpan;}}

    public static GeneratedArea CurrentCave { get => currentCave; set => currentCave = value; }
    private static GeneratedArea currentCave;
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
        if (sprite == null) {
            return Vector2Int.zero;
        }
        return new Vector2Int((int) (sprite.rect.width / Global.PixelsPerBlock), (int) (sprite.rect.height / Global.PixelsPerBlock));
    }

    public static Vector2Int Vector3IntToVector2Int(Vector3Int vector3Int) {
        return new Vector2Int(vector3Int.x, vector3Int.y);
    }

    public static Vector2Int getChunkFromWorld(Vector2 position) {
        return new Vector2Int(Mathf.FloorToInt(position.x/(Global.ChunkSize/2)), Mathf.FloorToInt(position.y/(Global.ChunkSize/2)));
    }
    public static Vector2Int getChunkFromCell(Vector2Int cellPosition) {
        return new Vector2Int(Mathf.FloorToInt(((float)cellPosition.x)/(Global.ChunkSize)), Mathf.FloorToInt(((float) cellPosition.y)/(Global.ChunkSize)));
    }
    public static Vector2Int getPartitionFromCell(Vector2Int cellPosition) {
        return new Vector2Int(Mathf.FloorToInt(((float) cellPosition.x)/(Global.ChunkPartitionSize)), Mathf.FloorToInt(((float)cellPosition.y)/(Global.ChunkPartitionSize)));
    }

    public static Vector2Int getPositionInPartition(Vector2Int cellPosition)
    {
        int partitionX = Mathf.Abs(cellPosition.x) % (Global.chunkPartitionSize);
        int partitionY = Mathf.Abs(cellPosition.y) % (Global.chunkPartitionSize);
        return new Vector2Int(partitionX, partitionY);
    }

    public static Vector2Int getPositionInChunk(Vector2Int cellPosition)
    {
        int chunkX = Mathf.Abs(cellPosition.x) % (Global.ChunkSize);
        int chunkY = Mathf.Abs(cellPosition.y) % (Global.ChunkSize);
        return new Vector2Int(chunkX, chunkY);

    }


    public static Vector2Int getPartitionFromWorld(Vector2 position) {
        return new Vector2Int(Mathf.FloorToInt(position.x/(Global.chunkPartitionSize/2)), Mathf.FloorToInt(position.y/(Global.chunkPartitionSize/2)));
    }
    public static Vector2Int getCellPositionFromWorld(Vector2 position) {
        return new Vector2Int(Mathf.FloorToInt(2*position.x), Mathf.FloorToInt(2*position.y));
    }


}