using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.TileMap
{
    public class ShaderTilemapManager
    {
        private readonly bool hasCollider;
        private readonly float zOffset;
        private readonly Transform parentTransform;
        private readonly Dictionary<Material, Tilemap> materialTileMaps = new();
        private readonly Stack<Tilemap> unusedTileMaps = new();
        private readonly List<Tilemap> usedTileMaps = new();
        public ShaderTilemapManager(Transform parent, float zOffset, bool collider, int defaultCount = 3)
        {
            this.hasCollider = collider;
            this.zOffset = zOffset;
            this.parentTransform = parent;
            int i = 0;
            while (i < defaultCount)
            {
                PushNewTilemap();
                i++;
            }
        }   
        
        private void PushNewTilemap()
        {
            int count = unusedTileMaps.Count + materialTileMaps.Count;
            GameObject overlayTileMapObject = new GameObject($"ShaderOverlayTileMap_{count}");
            overlayTileMapObject.transform.SetParent(parentTransform,false);
            var overlayTileMap = overlayTileMapObject.AddComponent<Tilemap>();
            overlayTileMapObject.AddComponent<TilemapRenderer>();
            overlayTileMapObject.transform.localPosition = new Vector3(0, 0, zOffset);
            
            if (hasCollider)
            {
                TilemapCollider2D collider2D = overlayTileMapObject.AddComponent<TilemapCollider2D>();
                collider2D.usedByComposite = true;
            }
            overlayTileMapObject.gameObject.SetActive(false);
            unusedTileMaps.Push(overlayTileMap);
        }
        
        public Tilemap GetTileMap(Material material)
        {
            if (materialTileMaps.TryGetValue(material, out Tilemap tilemap)) return tilemap;
            if (unusedTileMaps.Count == 0)
            {
                PushNewTilemap();
            }
            tilemap = unusedTileMaps.Pop();
            tilemap.gameObject.SetActive(true);
            tilemap.GetComponent<TilemapRenderer>().material = material;
            materialTileMaps[material] = tilemap;
            usedTileMaps.Add(tilemap);
            
            return tilemap;
        }

        public void ClearAllOnTile(ref Vector3Int cellPosition)
        {
            foreach (Tilemap tilemap in usedTileMaps)
            {
                if (tilemap.HasTile(cellPosition))
                {
                    tilemap.SetTile(cellPosition, null);
                    break;
                }
            }
        }
    }
}