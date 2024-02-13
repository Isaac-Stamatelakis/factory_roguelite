using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMapModule.Place;

[CreateAssetMenu(fileName ="New Rotatable Tile",menuName="Tile/RotatableTile")]
public class RotatableTile : Tile {
    public int rotation = 0;
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref UnityEngine.Tilemaps.TileData tileData) {
        Matrix4x4 matrix4X4 = tileData.transform;
        UnityEngine.Vector2Int spriteSize = new UnityEngine.Vector2Int(Mathf.FloorToInt(2* sprite.bounds.size.x), Mathf.FloorToInt(2* sprite.bounds.size.y));
        matrix4X4.SetTRS(Vector3.zero,Quaternion.Euler(0f, 0f, rotation*90f),Vector3.one);
        if (spriteSize.x > 1 || spriteSize.y > 1) {
            matrix4X4.m03 += 0.25f * ((int)(PlaceTile.mod((int)spriteSize.x+1,2)));
            matrix4X4.m13 += 0.25f * ((int)(PlaceTile.mod((int)spriteSize.y+1,2)));
        }            
        base.GetTileData(position, tilemap, ref tileData);
    }
}



public class TileSetter {
    public static Matrix4x4 setTile(UnityEngine.Tilemaps.TileData tileData, Sprite sprite) {
        Matrix4x4 matrix4X4 = tileData.transform;
        UnityEngine.Vector2Int spriteSize = new UnityEngine.Vector2Int(Mathf.FloorToInt(2* sprite.bounds.size.x), Mathf.FloorToInt(2* sprite.bounds.size.y));
        //matrix4X4.SetTRS(Vector3.zero,Quaternion.Euler(0f, 0f, rotation*90f),Vector3.one);
        if (spriteSize.x > 1 || spriteSize.y > 1) {
            matrix4X4.m03 += 0.25f * ((int)(PlaceTile.mod((int)spriteSize.x+1,2)));
            matrix4X4.m13 += 0.25f * ((int)(PlaceTile.mod((int)spriteSize.y+1,2)));
        }            
        return matrix4X4;
    }
}