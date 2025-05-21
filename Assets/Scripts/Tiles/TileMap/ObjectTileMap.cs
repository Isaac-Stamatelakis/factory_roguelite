using Chunks.Partitions;
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
        public override void Initialize(TileMapType type)
        {
            base.Initialize(type);
            overlayTileMap = AddOverlay();
        }

        protected override void SetTile(int x, int y, TileItem tileItem)
        {
            base.SetTile(x, y, tileItem);
            TileOverlay tileOverlay = tileItem.tileOptions.Overlay;
            if (!tileOverlay) return;
            
            Vector2Int position = new Vector2Int(x, y);
            IChunkPartition partition = GetPartitionAtPosition(position);
            if (partition == null) return; // Might need this?
            Vector2Int positionInPartition = GetTilePositionInPartition(position);
            BaseTileData baseTileData = partition.GetBaseData(positionInPartition);
            PlaceOverlayTile(tileItem.tileOptions.Overlay,overlayTileMap, new Vector3Int(x,y,0),tileItem,baseTileData);
        }
        

        protected override void RemoveTile(int x, int y)
        {
            base.RemoveTile(x, y);
            Vector3Int cellPosition = new Vector3Int(x, y, 0);
            overlayTileMap.SetTile(cellPosition, null);
        }

        
        
    }
}
