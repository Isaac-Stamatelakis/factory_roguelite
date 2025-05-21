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
    public class MousePositionStateTileSingleStateTorchTile : TileBase, IMousePositionStateTile, IStateTileSingle, INoDelayPreviewTile, IDirectionStateTile
    {
        private enum TorchTileState
        {
            Invalid = -1,
            OnGround = 0,
            WallLeft = 1,
            WallRight = 2,
            Background = 3,
        }
        public Tile onBlock;
        public Tile onLeft;
        public Tile onRight;
        public Tile onBackground;

       
       
        public int GetStateAtPosition(Vector2 position) {
            bool down = PlaceTile.tileInDirection(position,Direction.Down,TileMapLayer.Base);
            
            int mousePosition = MousePositionUtils.GetMousePlacement(position);
            if (down && MousePositionUtils.MouseCentered(true,position)) {
                return (int)TorchTileState.OnGround;
            }
            bool left = PlaceTile.tileInDirection(position,Direction.Left,TileMapLayer.Base);
            // If top 
            if (left && MousePositionUtils.MouseBiasDirection(mousePosition,MousePlacement.Left)) {
                return (int)TorchTileState.WallLeft;
            }
            bool right = PlaceTile.tileInDirection(position,Direction.Right,TileMapLayer.Base);
            if (right && MousePositionUtils.MouseBiasDirection(mousePosition,MousePlacement.Right)) {
                return (int)TorchTileState.WallRight;
            }
            
            bool background = PlaceTile.tileInDirection(position,Direction.Center,TileMapLayer.Background);
            if (!left && !right && !down && background) {
                return (int)TorchTileState.Background;
            }
            if (down) {
                return (int)TorchTileState.OnGround;
            }
            if (left) {
                return (int)TorchTileState.WallLeft;
            }
            if (right) {
                return (int)TorchTileState.WallRight;
            }
            return (int)TorchTileState.Invalid;
        }

        public TileBase GetTileAtState(int state)
        {
            TorchTileState torchTileState = (TorchTileState)state;
            switch (torchTileState) {
                case TorchTileState.OnGround:
                    return onBlock;
                case TorchTileState.WallLeft:
                    return onLeft;
                case TorchTileState.WallRight:
                    return onRight;
                case TorchTileState.Background:
                    return onBackground;
                case TorchTileState.Invalid:
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

