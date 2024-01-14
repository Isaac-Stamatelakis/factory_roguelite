using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class Global
{
    private static int rotation = 2;
    public static int Rotation {get{return rotation;} set{rotation = value;}}
    private static float inventoryScale = 1.2f;
    public static float InventoryScale {get{return inventoryScale;}}
    private static string worldName = "world0";
    public static string WorldName {get{return worldName;}}
    private static float pixelsPerBlock = 16;
    public static float PixelsPerBlock {get {return pixelsPerBlock;}}

    private static int chunkSize = 16;
    public static int ChunkSize {get{return chunkSize;}}
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
    private static int chunkLoadRange = 2;
    public static int ChunkLoadRange {get {return chunkLoadRange;}}
    private static float tileItemEntityScalar = 0.5f;
    private static int maxSize = 100;
    public static int MaxSize {get{return maxSize;}}
    public static float TileItemEntityScalar {get {return tileItemEntityScalar;}}

    private static float itemEntityLifeSpan = 300f;
    public static float ItemEntityLifeSpawn {get{return itemEntityLifeSpan;}}
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
    public static Vector2 getSpriteSize(int id) {
        Sprite sprite = IdDataMap.getInstance().GetSprite(id);
        return new Vector2((int) (sprite.texture.width / Global.PixelsPerBlock), (int) (sprite.rect.height / Global.PixelsPerBlock));
    }

    public static Vector2 getSpriteSize(Sprite sprite) {
        return new Vector2((int) (sprite.texture.width / Global.PixelsPerBlock), (int) (sprite.rect.height / Global.PixelsPerBlock));
    }

    public static Vector2Int Vector3IntToVector2Int(Vector3Int vector3Int) {
        return new Vector2Int(vector3Int.x, vector3Int.y);
    }

    public static void setStatic(GameObject anObject) {
        GameObjectUtility.SetStaticEditorFlags(anObject, StaticEditorFlags.NavigationStatic | StaticEditorFlags.BatchingStatic);
    }
    public static Vector2Int getChunk(Vector2 position) {
        return new Vector2Int(Mathf.FloorToInt(position.x/8f),Mathf.FloorToInt(position.y/8f));
    }


}
