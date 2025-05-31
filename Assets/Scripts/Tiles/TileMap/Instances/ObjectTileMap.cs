using Chunks.Partitions;
using Items;
using Items.Transmutable;
using TileMaps;
using TileMaps.Type;
using Tiles.Options.Overlay;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.TileMap
{
    public class ObjectTileMap : WorldTileMap
    {
        private Tilemap overlayTileMap;
        private ShaderTilemapManager shaderTilemapManager;
        public override void Initialize(TileMapType type)
        {
            base.Initialize(type);
            overlayTileMap = AddOverlay(OVERLAY_Z);
            shaderTilemapManager = new ShaderTilemapManager(transform, OVERLAY_Z, false,0);
        }

        protected override void SetTile(int x, int y, TileItem tileItem)
        {
            Vector2Int position = new Vector2Int(x, y);
            IChunkPartition partition = GetPartitionAtPosition(position);
            if (partition == null) return; // Might need this?
            
            TileOverlay tileOverlay = tileItem.tileOptions.Overlay;
            Tilemap placementTilemap = GetPlacementTilemap(tileItem);
            Vector3Int placementPositon = new Vector3Int(x, y, 0);
            PlaceTileInTilemap(placementTilemap,tileItem,placementPositon,partition);
            if (!tileOverlay) return;
            
            Vector2Int positionInPartition = GetTilePositionInPartition(position);
            BaseTileData baseTileData = partition.GetBaseData(positionInPartition);
            PlaceOverlayTile(tileItem.tileOptions.Overlay,overlayTileMap, new Vector3Int(x,y,0),tileItem,baseTileData);
        }

        private Tilemap GetPlacementTilemap(TileItem tileItem)
        {
            var transmutableMaterial = tileItem.tileOptions.TransmutableColorOverride;
            if (!transmutableMaterial) return tilemap;
            Material material = ItemRegistry.GetInstance().GetTransmutationWorldMaterial(transmutableMaterial);
            return material ? shaderTilemapManager.GetTileMap(material) : tilemap;
        }
        

        protected override void RemoveTile(int x, int y)
        {
            Vector3Int cellPosition = new Vector3Int(x, y, 0);
            tilemap.SetTile(cellPosition,null);
            overlayTileMap.SetTile(cellPosition, null);
            shaderTilemapManager.ClearAllOnTile(ref cellPosition);
        }

        
        
    }
}
