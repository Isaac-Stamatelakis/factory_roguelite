using TileMaps.Layer;
using TileMaps.Place;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.CustomTiles.StateTiles.Instances {
    
    [CreateAssetMenu(fileName ="T~Sign Tile",menuName="Tile/State/Sign")]
    public class RestrictedSignTile : TileBase, IMousePositionStateTile, IStateTileSingle
    {
        public Tile onBlock;
        public Tile onLeft;
        public Tile onRight;
        public Tile hanging;
        
        public int GetStateAtPosition(Vector2 position) {
            // If exists tile to left place left
            // If exists tile to right place right
            // If exists tile on bottom place bottom
            // If exists tile on background place background
            // If no condition return -1
            bool left = TilePlaceUtils.TileInDirection(position,Direction.Left,TileMapLayer.Base);
            bool right = TilePlaceUtils.TileInDirection(position,Direction.Right,TileMapLayer.Base);
            bool down = TilePlaceUtils.TileInDirection(position,Direction.Down,TileMapLayer.Base);
            bool up = TilePlaceUtils.TileInDirection(position,Direction.Up,TileMapLayer.Base);
            
            // Priotize placing down
            int mousePosition = MousePositionUtils.GetMousePlacement(position);
            if (MousePositionUtils.MouseBiasDirection(mousePosition,MousePlacement.Down) && !left && !right && down) {
                return 0;
            }
            if (MousePositionUtils.MouseBiasDirection(mousePosition,MousePlacement.Left) && left) {
                return 1;
            }
            if (MousePositionUtils.MouseBiasDirection(mousePosition,MousePlacement.Right) && right) {
                return 2;
            }
            if (!left && !right && !down && up) {
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
            if (up) {
                return 3;
            }
            return -1;
        }

        public TileBase GetTileAtState(int state)
        {
            switch (state) {
                case 0:
                    return onBlock;
                case 1:
                    return onLeft;
                case 2:
                    return onRight;
                case 3:
                    return hanging;
                default:
                    return null;
            }
        }

        public TileBase GetDefaultTile()
        {
            return onBlock;
        }

        public int GetStateAmount()
        {
            return 4;
        }
    }
}

