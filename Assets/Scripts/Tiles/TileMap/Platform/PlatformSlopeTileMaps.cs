using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.TileMap.Platform
{
    public enum SlopeRotation
    {
        Left,
        Right
    }
    public class PlatformSlopeTileMaps
    {
        private readonly Tilemap slopeTileMap;
        private readonly Tilemap decoTileMap;
        private readonly Tilemap extraColliderTilemap;
        private readonly TileBase extraColliderTile;
        private readonly Vector3Int extendTileDirection;

        public PlatformSlopeTileMaps(Tilemap slopeTilemap, Tilemap decoTileMap, Tilemap extraColliderTilemap,  TileBase extraColliderTile, SlopeRotation slopeRotation)
        {
            this.slopeTileMap = slopeTilemap;
            this.decoTileMap = decoTileMap;
            this.extraColliderTilemap = extraColliderTilemap;
            this.extraColliderTile = extraColliderTile;
            extendTileDirection = slopeRotation == SlopeRotation.Left ? Vector3Int.right : Vector3Int.left;
        }

        public Tilemap GetSlopeTileMap()
        {
            return slopeTileMap;
        }

        public void Clear(Vector3Int cellPosition)
        {
            if (!slopeTileMap.GetTile(cellPosition)) return;
            slopeTileMap.SetTile(cellPosition, null);
            decoTileMap.SetTile(cellPosition+Vector3Int.down, null);
            
            extraColliderTilemap.SetTile(cellPosition+extendTileDirection, null); 
        }

        public void SetTile(ref Vector3Int cellPosition, TileBase slopeTile, TileBase decoTile)
        {
            slopeTileMap.SetTile(cellPosition, slopeTile);
            decoTileMap.SetTile(cellPosition + Vector3Int.down, decoTile);
            extraColliderTilemap.SetTile(cellPosition+extendTileDirection, extraColliderTile);
            
        }

        public void SetTransformMatrix(ref Vector3Int cellPosition, ref Matrix4x4 transformMatrix)
        {
            slopeTileMap.SetTransformMatrix(cellPosition,transformMatrix);
            decoTileMap.SetTransformMatrix(cellPosition+Vector3Int.down,transformMatrix);
            extraColliderTilemap.SetTransformMatrix(cellPosition+extendTileDirection,transformMatrix);
        }

        public void SetColor(ref Vector3Int cellPosition, ref Color color)
        {
            slopeTileMap.SetTileFlags(cellPosition, TileFlags.None);
            slopeTileMap.SetColor(cellPosition,color);
            decoTileMap.SetTileFlags(cellPosition+Vector3Int.down, TileFlags.None);
            decoTileMap.SetColor(cellPosition+Vector3Int.down,color);
        }
    }
}
