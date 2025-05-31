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
            shaderTilemapManager = new ShaderTilemapManager(transform, OVERLAY_Z, false,TileMapType.Object,0);
        }

        protected override void SetTile(int x, int y, TileItem tileItem)
        {
            Vector2Int position = new Vector2Int(x, y);
            IChunkPartition partition = GetPartitionAtPosition(position);
            if (partition == null) return; // Might need this?
            
            TileOverlay tileOverlay = tileItem.tileOptions.Overlay;
            var transmutableMaterial = tileItem.tileOptions.TransmutableColorOverride;
            Vector3Int placementPositon = new Vector3Int(x, y, 0);
            
            // Always place a tile in the primary map. If it has a material will be covered by material overlay map
            // But many operations rely on a tile being in the primary tilemap
            PlaceTileInTilemap(tilemap, tileItem, placementPositon, partition); 
            
            if (transmutableMaterial)
            {
                Material material = ItemRegistry.GetInstance().GetTransmutationWorldMaterial(transmutableMaterial);
                if (material)
                {
                    PlaceTileInTilemap(shaderTilemapManager.GetTileMap(material), tileItem, placementPositon, partition);
                }
            }

            if (!tileOverlay) return;
            
            Vector2Int positionInPartition = GetTilePositionInPartition(position);
            BaseTileData baseTileData = partition.GetBaseData(positionInPartition);
            PlaceOverlayTile(tileItem.tileOptions.Overlay,overlayTileMap, new Vector3Int(x,y,0),tileItem,baseTileData);
        }

        
        protected override void RemoveTile(int x, int y)
        {
            Vector3Int cellPosition = new Vector3Int(x, y, 0);
            tilemap.SetTile(cellPosition,null);
            overlayTileMap.SetTile(cellPosition, null);
            shaderTilemapManager.ClearAllOnTile(ref cellPosition);
        }

        public override bool HasTile(Vector3Int vector3Int)
        {
            return tilemap.HasTile(vector3Int);
        }
    }
}
