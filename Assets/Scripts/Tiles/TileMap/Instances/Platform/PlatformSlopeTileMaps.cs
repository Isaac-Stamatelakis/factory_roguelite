using System.Collections.Generic;
using TileMaps.Type;
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
        private readonly ShaderTilemapManager slopeShaderTilemapManager;
        private readonly ShaderTilemapManager decoShaderTilemapManager;
        private readonly Tilemap slopeTileMap;
        private readonly Tilemap decoTileMap;
        private readonly Tilemap extraColliderTilemap;
        private readonly TileBase extraColliderTile;
        private readonly Vector3Int extendTileDirection;

        public PlatformSlopeTileMaps(Tilemap slopeTilemap, Tilemap decoTileMap, Tilemap extraColliderTilemap,  TileBase extraColliderTile, SlopeRotation slopeRotation, ShaderTilemapManager slopeShaders, ShaderTilemapManager decoShaders)
        {
            this.slopeTileMap = slopeTilemap;
            this.decoTileMap = decoTileMap;
            this.extraColliderTilemap = extraColliderTilemap;
            this.extraColliderTile = extraColliderTile;
            extendTileDirection = slopeRotation == SlopeRotation.Left ? Vector3Int.right : Vector3Int.left;
            slopeShaderTilemapManager = slopeShaders;
            decoShaderTilemapManager = decoShaders;
        }

        public Tilemap GetSlopeTileMap()
        {
            return slopeTileMap;
        }

        public void Clear(Vector3Int cellPosition, Material material)
        {
            Tilemap slopePlacementMap = !material ? slopeTileMap : slopeShaderTilemapManager.GetTileMap(material);
            Tilemap decoPlacementMap = !material ? decoTileMap : decoShaderTilemapManager.GetTileMap(material);
            
            slopePlacementMap.SetTile(cellPosition, null);
            decoPlacementMap.SetTile(cellPosition+Vector3Int.down, null);
            
            extraColliderTilemap.SetTile(cellPosition+extendTileDirection, null); 
        }

        public void SetTile(ref Vector3Int cellPosition, TileBase slopeTile, TileBase decoTile, ref Matrix4x4 transformMatrix, ref Color color, Material material)
        {
            bool hasMaterial = material;
            Tilemap decoPlacementMap = !material ? decoTileMap : decoShaderTilemapManager.GetTileMap(material);
            
            slopeTileMap.SetTile(cellPosition, slopeTile);
            slopeTileMap.SetTransformMatrix(cellPosition,transformMatrix);
            
            decoPlacementMap.SetTile(cellPosition + Vector3Int.down, decoTile);
            decoPlacementMap.SetTransformMatrix(cellPosition+Vector3Int.down,transformMatrix);
            
            extraColliderTilemap.SetTile(cellPosition+extendTileDirection, extraColliderTile);
            extraColliderTilemap.SetTransformMatrix(cellPosition+extendTileDirection,transformMatrix);

            bool hasColor = color != Color.white;
            if (hasColor)
            {
                decoPlacementMap.SetTileFlags(cellPosition+Vector3Int.down, TileFlags.None);
                decoPlacementMap.SetColor(cellPosition+Vector3Int.down,color);
                if (!hasMaterial)
                {
                    slopeTileMap.SetTileFlags(cellPosition, TileFlags.None);
                    slopeTileMap.SetColor(cellPosition,color);
                }
            }

            if (!hasMaterial) return;
            
            Tilemap shaderMap = slopeShaderTilemapManager.GetTileMap(material);
            shaderMap.SetTile(cellPosition, slopeTile);
            shaderMap.SetTransformMatrix(cellPosition,transformMatrix);
            if (!hasColor) return;
            shaderMap.SetTileFlags(cellPosition, TileFlags.None);
            shaderMap.SetColor(cellPosition,color);
            
        }

        public void AddShaperMapsToList(List<ShaderTilemapManager> managers)
        {
            managers.Add(slopeShaderTilemapManager);
            managers.Add(decoShaderTilemapManager);
        }
    }
}
