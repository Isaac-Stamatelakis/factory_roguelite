using System.Collections;
using System.Collections.Generic;
using Chunks.Partitions;
using Items;
using UnityEngine;
using Tiles;
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
            outline.transform.SetParent(transform,false);
            setView(false,Color.black);
        }

        public void setView(bool wireFrame, Color color) {
            float z = wireFrame ? -0.1f : 0.1f;
            outlineTileMap.transform.localPosition = new Vector3(0,0,z);
            outlineTileMap.color = color;
        }


        protected override void breakTile(Vector2Int position)
        {
            outlineTileMap.SetTile(new Vector3Int(position.x,position.y,0), null);
            base.breakTile(position);
        }

        protected override void removeTile(int x, int y)
        {
            outlineTileMap.SetTile(new Vector3Int(x,y,0), null);
            base.removeTile(x, y);
        }

        protected override void setTile(int x, int y, TileItem tileItem)
        {
            base.setTile(x, y, tileItem);
            TileBase outlineTile = tileItem.outline;
            if (outlineTile == null) {
                return;
            }
            if (outlineTile is not IStateTile stateTile) {
                outlineTileMap.SetTile(new Vector3Int(x,y,0),outlineTile);
                return;
            }
            Vector3Int vec3 = new Vector3Int(x,y,0);
            TileOptions tileOptions = getOptionsAtPosition(new Vector2Int(x,y));
            SerializedTileOptions serializedTileOptions = tileOptions.SerializedTileOptions;
            int state = serializedTileOptions.state;

            TileBase stateOutlineTile = stateTile.getTileAtState(state);
            outlineTileMap.SetTile(vec3,stateOutlineTile);

            int rotation = serializedTileOptions.rotation;
            bool mirror = serializedTileOptions.mirror;
            bool rotate = rotation > 0 || mirror;
            if (!rotate) {
                return;
            }
            Matrix4x4 transformMatrix = outlineTileMap.GetTransformMatrix(vec3);
            if (!mirror) {
                transformMatrix.SetTRS(Vector3.zero, Quaternion.Euler(0f, 0f, 90*rotation), Vector3.one);
            } else {
                transformMatrix.SetTRS(Vector3.zero, Quaternion.Euler(0f, 180f, 90*rotation), Vector3.one);
            }
            outlineTileMap.SetTransformMatrix(vec3,transformMatrix);
        }
    }
}

