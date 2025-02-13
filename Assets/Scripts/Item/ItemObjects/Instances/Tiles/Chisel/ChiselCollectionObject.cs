using System.Collections.Generic;
using Dimensions;
using TileMaps;
using TileMaps.Layer;
using TileMaps.Type;
using UnityEngine;

namespace Item.ItemObjects.Instances.Tile.Chisel
{
    [CreateAssetMenu(fileName ="I~New Tile Item",menuName="Item/Instances/Chisel/Collection")]
    public class ChiselCollectionObject : ScriptableObject
    {
        public List<ChiselTileItem> ChiselTiles;
    }

    public static class ChiselItemUtils
    {
        public static ChiselTileItem Iterate(int direction, ChiselTileItem chiselTileItem)
        {
            ChiselCollectionObject chiselCollectionObject = chiselTileItem.Collection;
            int index = chiselCollectionObject.ChiselTiles.IndexOf(chiselTileItem);
            if (index == -1)
            {
                Debug.LogWarning("Chisel tile item not in collection");
                return null;
            }
            int count = chiselCollectionObject.ChiselTiles.Count;
            int newIndex = ((index + direction) % count + count) % count;
            return chiselCollectionObject.ChiselTiles[newIndex];
        }

        public static void TryIterateChiselItem(Vector2 mousePosition, int direction)
        {
            Transform playerTransform = PlayerManager.Instance.GetPlayer().transform;
            IWorldTileMap iWorldTileMap = DimensionManager.Instance.GetPlayerSystem().GetTileMap(TileMapType.Block);
            if (iWorldTileMap is not IChiselableTileMap chiselableTileMap) return;
            Vector3Int cellPosition = iWorldTileMap.GetTilemap().WorldToCell(mousePosition);
            chiselableTileMap.IterateChiselTile((Vector2Int)cellPosition, direction);
        }
    }
}
