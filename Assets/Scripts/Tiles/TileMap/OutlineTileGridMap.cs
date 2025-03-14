using System.Collections;
using System.Collections.Generic;
using Chunks.Partitions;
using Items;
using TileMaps.Type;
using UnityEngine;
using Tiles;
using UnityEngine.Tilemaps;

namespace TileMaps {
    public struct OutlineTileMapCellData
    {
        public TileBase Tile;
        public TileBase OutlineTile;
        public Quaternion OutlineRotation;
        public Quaternion TileRotation;

        public OutlineTileMapCellData(TileBase tile, TileBase outlineTile, Quaternion tileRotation, Quaternion outlineRotation)
        {
            Tile = tile;
            OutlineTile = outlineTile;
            OutlineRotation = outlineRotation;
            TileRotation = tileRotation;
        }
    }
    public interface IOutlineTileGridMap
    {
        public OutlineTileMapCellData GetOutlineCellData(Vector3Int position);
    }
    public class OutlineWorldTileGridMap : WorldTileGridMap, IOutlineTileGridMap
    {
        private Tilemap outlineTileMap;
        public override void Initialize(TileMapType tileMapType)
        {
            base.Initialize(tileMapType);
            GameObject outline = new GameObject();
            outline.name = "Outline";
            outline.AddComponent<TilemapRenderer>();
            outlineTileMap = outline.GetComponent<Tilemap>();
            outline.transform.SetParent(transform,false);
            setView(false,Color.black);
        }

        public void setView(bool? wireFrame, Color? color) {
            if (wireFrame != null)
            {
                float z =(bool)wireFrame ? -0.1f : 0.6f;
                outlineTileMap.transform.localPosition = new Vector3(0,0,z);
            }
            
            if (color != null) {
                outlineTileMap.color = (Color)color;
            }
            
        }


        public override void BreakTile(Vector2Int position)
        {
            base.BreakTile(position);
            outlineTileMap.SetTile(new Vector3Int(position.x,position.y,0), null);
        }

        protected override void RemoveTile(int x, int y)
        {
            outlineTileMap.SetTile(new Vector3Int(x,y,0), null);
            base.RemoveTile(x, y);
        }

        protected override void SetTile(int x, int y, TileItem tileItem)
        {
            base.SetTile(x, y, tileItem);
            TileBase outlineTile = tileItem.outline;
            if (ReferenceEquals(outlineTile,null)) {
                return;
            }
            if (outlineTile is not IStateTile stateTile) {
                outlineTileMap.SetTile(new Vector3Int(x,y,0),outlineTile);
                return;
            }
            Vector3Int vec3 = new Vector3Int(x,y,0);
            Vector2Int tilePosition = new Vector2Int(x, y);
            IChunkPartition partition = GetPartitionAtPosition(tilePosition);
            Vector2Int tilePositionInPartition = GetTilePositionInPartition(tilePosition);
            if (partition == null) return;
            BaseTileData baseTileData = partition.GetBaseData(tilePositionInPartition);
            int state = baseTileData.state;

            TileBase stateOutlineTile = stateTile.getTileAtState(state);
            outlineTileMap.SetTile(vec3,stateOutlineTile);

            int rotation = baseTileData.rotation;
            bool mirror = baseTileData.mirror;
            bool rotate = rotation > 0 || mirror;
            if (!rotate) {
                return;
            }
            Matrix4x4 transformMatrix = outlineTileMap.GetTransformMatrix(vec3);
            transformMatrix.SetTRS(Vector3.zero, !mirror ? Quaternion.Euler(0f, 0f, 90 * rotation) : Quaternion.Euler(0f, 180f, 90 * rotation), Vector3.one);
            outlineTileMap.SetTransformMatrix(vec3,transformMatrix);
        }

        public OutlineTileMapCellData GetOutlineCellData(Vector3Int position)
        {
            return new OutlineTileMapCellData(tilemap.GetTile(position),outlineTileMap.GetTile(position),tilemap.GetTransformMatrix(position).rotation,outlineTileMap.GetTransformMatrix(position).rotation);
        }
    }
}

