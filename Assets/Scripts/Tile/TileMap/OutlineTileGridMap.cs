using System.Collections;
using System.Collections.Generic;
using Chunks.Partitions;
using Items;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileMaps {
    public class OutlineTileGridMap : TileGridMap
    {
        private Tilemap outlineTileMap;
        public override void Start()
        {
            
            base.Start();
            GameObject outline = new GameObject();
            outline.name = "Outline";
            outline.AddComponent<TilemapRenderer>();
            outlineTileMap = outline.GetComponent<Tilemap>();
            outline.transform.SetParent(transform);
            
        }

        protected override void breakTile(Vector2Int position)
        {
            base.breakTile(position);
            outlineTileMap.SetTile((Vector3Int)position,null);
        }

        protected override void setTile(int x, int y, TileItem tileItem)
        {
            base.setTile(x, y, tileItem);
            TileBase outlineTile = tileItem.outline;
            outlineTileMap.SetTile(new Vector3Int(x,y,0),outlineTile);
        }
    }
}

