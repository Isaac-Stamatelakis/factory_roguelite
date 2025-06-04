using System.Collections.Generic;
using System.Linq;
using TileMaps.Type;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.TileMap
{
    public interface IWorldShaderTilemap
    {
        public ShaderTilemapManager GetManager();
    }
    public class ShaderTilemapManager
    {
        private const string UNUSED_NAME = "UnusedTileMap";
        private readonly bool hasCollider;
        private readonly float zOffset;
        private readonly Transform parentTransform;
        private readonly Dictionary<int, Tilemap> materialTileMaps = new();
        private readonly Stack<Tilemap> unusedTileMaps = new();
        private readonly List<Tilemap> usedTileMaps = new();
        private TileMapType tileMapType;
        public ShaderTilemapManager(Transform parent, float zOffset, bool collider, TileMapType tileMapType, int defaultCount = 3)
        {
            this.hasCollider = collider;
            this.zOffset = zOffset;
            this.parentTransform = parent;
            this.hasCollider = collider;
            this.tileMapType = tileMapType;
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
            GameObject overlayTileMapObject = new GameObject(UNUSED_NAME);
            overlayTileMapObject.layer = LayerMask.NameToLayer(tileMapType.ToString());
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
            if (materialTileMaps.TryGetValue(material.GetInstanceID(), out Tilemap tilemap)) return tilemap;
            if (unusedTileMaps.Count == 0)
            {
                PushNewTilemap();
            }
            tilemap = unusedTileMaps.Pop();
            tilemap.gameObject.SetActive(true);
            tilemap.GetComponent<TilemapRenderer>().material = material;
            tilemap.name = $"{material.name}_tilemap";
            materialTileMaps[material.GetInstanceID()] = tilemap;
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

        public void ClearAllTiles()
        {
            foreach (Tilemap tilemap in usedTileMaps)
            {
                tilemap.ClearAllTiles();
            }
        }

        public void PushUnusedMaps()
        {
            List<Tilemap> removedTilemaps = new List<Tilemap>();
            for (var index = usedTileMaps.Count-1; index >= 0 ; index--)
            {
                var tilemap = usedTileMaps[index];
                if (!tilemap) continue;
                tilemap.CompressBounds();
                if (tilemap.cellBounds.size.x > 0 || tilemap.cellBounds.size.y > 0)
                {
                    continue; 
                }
                tilemap.gameObject.SetActive(false);
                unusedTileMaps.Push(tilemap);
                usedTileMaps.RemoveAt(index);
                removedTilemaps.Add(tilemap);
                
                tilemap.gameObject.name = UNUSED_NAME;
            }

            // Removing by material is not consistent due to Instance copying. Just search for the key with the tilemap
            foreach (Tilemap tilemap in removedTilemaps)
            {
                var pair = materialTileMaps.FirstOrDefault(x => x.Value == tilemap);
                if (pair.Value)
                {
                    materialTileMaps.Remove(pair.Key);
                }
            }
        }
    }
}