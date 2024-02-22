using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMapModule.Place;
using TileMapModule.Layer;

namespace Tiles {
    
    [CreateAssetMenu(fileName ="T~Torch Tile",menuName="Tile/Torch")]
    public class RestrictedTorchTile : TileBase, IRestrictedTile, IIDTile
    {
        public string id;
        public Tile onBlock;
        public Tile onLeft;
        public Tile onRight;
        public Tile onBackground;
        public Sprite getSprite()
        {
            return onBlock.sprite;
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref UnityEngine.Tilemaps.TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
        }

        public string getId()
        {
            return id;
        }

        public void setID(string id)
        {
            this.id = id;
        }
        public int getStateAtPosition(Vector2 position,VerticalMousePosition verticalMousePosition, HorizontalMousePosition horizontalMousePosition) {
            // If exists tile to left place left
            // If exists tile to right place right
            // If exists tile on bottom place bottom
            // If exists tile on background place background
            // If no condition return -1
            bool left = PlaceTile.tileInDirection(position,Direction.Left,TileMapLayer.Base);
            bool right = PlaceTile.tileInDirection(position,Direction.Right,TileMapLayer.Base);
            bool down = PlaceTile.tileInDirection(position,Direction.Down,TileMapLayer.Base);
            bool background = PlaceTile.tileInDirection(position,Direction.Center,TileMapLayer.Background);
            //Debug.Log("Left" + left + "," + "Right" + right + "," + "Down" + down + "," + "Background" + background);
            // Priotize placing down
            if (verticalMousePosition == VerticalMousePosition.Bottom && !left && !right && down) {
                return 0;
            }
            // If top 
            if (horizontalMousePosition == HorizontalMousePosition.Left && left) {
                return 1;
            }
            if (horizontalMousePosition == HorizontalMousePosition.Right && right) {
                return 2;
            }
            if (!left && !right && !down && background) {
                return 3;
            }
            if (down) {
                return 0;
            }
            if (left) {
                return 1;
            }
            if (right) {
                return 2;
            }
            if (background) {
                return 3;
            }
            return -1;
        }

        public TileBase getTileAtState(int state)
        {
            switch (state) {
                case 0:
                    return onBlock;
                case 1:
                    return onLeft;
                case 2:
                    return onRight;
                case 3:
                    return onBackground;
                default:
                    return null;
            }
        }

        public int getStateAmount()
        {
            return 4;
        }
    }
}

