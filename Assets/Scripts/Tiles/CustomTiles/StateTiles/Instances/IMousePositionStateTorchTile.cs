using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMaps.Place;
using TileMaps.Layer;

namespace Tiles {
    public interface INoDelayPreviewTile {
    
    }

    public interface IDirectionStateTile
    {
        public Direction? GetDirection(int state);
    }
    
    [CreateAssetMenu(fileName ="T~Torch Tile",menuName="Tile/State/Torch")]
    public class IMousePositionStateTorchTile : TileBase, IMousePositionStateTile, IIDTile, IStateTile, INoDelayPreviewTile, IDirectionStateTile
    {
        public string id;
        public Tile onBlock;
        public Tile onLeft;
        public Tile onRight;
        public Tile onBackground;

        public string getId()
        {
            return id;
        }

        public void setID(string id)
        {
            this.id = id;
        }
        public int GetStateAtPosition(Vector2 position) {
            // If exists tile to left place left
            // If exists tile to right place right
            // If exists tile on bottom place bottom
            // If exists tile on background place background
            // If no condition return -1
            Vector2 centered = TileHelper.getRealTileCenter(position);
            if (PlaceTile.raycastTileInBox(centered, TileMapLayer.Base.toRaycastLayers(), true)) return -1;
            bool left = PlaceTile.tileInDirection(position,Direction.Left,TileMapLayer.Base);
            bool right = PlaceTile.tileInDirection(position,Direction.Right,TileMapLayer.Base);
            bool down = PlaceTile.tileInDirection(position,Direction.Down,TileMapLayer.Base);
            bool background = PlaceTile.tileInDirection(position,Direction.Center,TileMapLayer.Background);
           
            int mousePosition = MousePositionUtils.GetMousePlacement(position);
            if (down && MousePositionUtils.MouseCentered(true,position)) {
                return 0;
            }
            // If top 
            if (left && MousePositionUtils.MouseBiasDirection(mousePosition,MousePlacement.Left)) {
                return 1;
            }
            if (right && MousePositionUtils.MouseBiasDirection(mousePosition,MousePlacement.Right)) {
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

        public TileBase GetDefaultTile()
        {
            return onBlock;
        }

        public int getStateAmount()
        {
            return 4;
        }

        public Direction? GetDirection(int state)
        {
            return state switch
            {
                0 => Direction.Down,
                1 => Direction.Left,
                2 => Direction.Right,
                _ => null
            };
        }
    }
}

