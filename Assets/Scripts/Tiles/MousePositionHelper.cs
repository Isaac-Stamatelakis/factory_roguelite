using System.Collections;
using System.Collections.Generic;
using TileMaps.Layer;
using TileMaps.Place;
using UnityEngine;

namespace Tiles {
    public enum MousePlacement
    {
        Left = 1,
        Right = 2,
        Up = 4,
        Down = 8,
    }

    public static class MousePositionUtils
    {
        const int NO_AUTO_ROTATION = -1;
        public static int CalculateHammerTileRotation(Vector2 mousePosition, int placementState)
        {
            if (placementState == 0) return NO_AUTO_ROTATION;
            List<Direction> directions = new List<Direction>
                { Direction.Left, Direction.Right, Direction.Down, Direction.Up };
            HashSet<Direction> directionsWithTile = new HashSet<Direction>();

            foreach (Direction direction in directions)
            {
                 if (TilePlaceUtils.TileInDirection(mousePosition, direction, TileMapLayer.Base)) directionsWithTile.Add(direction);
            }
            
            int mousePlacement = GetMousePlacement(mousePosition);
            switch (placementState)
            {
                case HammerTile.SLAB_TILE_STATE:
                    return CalculateSlabState(directionsWithTile, mousePlacement);
                case HammerTile.SLANT_TILE_STATE:
                case HammerTile.STAIR_TILE_STATE:
                    return CalculateSlantStairState(directionsWithTile, mousePlacement);
                default:
                    return NO_AUTO_ROTATION;
            }
            
            
        }

        private static int CalculateSlantStairState(HashSet<Direction> directionsWithTile, int mousePlacement)
        {
            bool biasLeft = (mousePlacement & (int)MousePlacement.Left) != 0;
            bool biasDown = (mousePlacement & (int)MousePlacement.Down) != 0;
            const int RD_ROT = 0;
            const int LD_ROT = 3;
            const int LU_ROT = 2;
            const int RU_ROT = 1;

            bool ld = directionsWithTile.Contains(Direction.Left) && directionsWithTile.Contains(Direction.Down);
            bool lu = directionsWithTile.Contains(Direction.Left) && directionsWithTile.Contains(Direction.Up);
            bool rd = directionsWithTile.Contains(Direction.Right) && directionsWithTile.Contains(Direction.Down);
            bool ru = directionsWithTile.Contains(Direction.Right) && directionsWithTile.Contains(Direction.Up);

            if (ld && lu && rd && ru)
            {
                if (biasLeft)
                {
                    return biasDown ? LD_ROT : LU_ROT;
                }
                return biasDown ? RD_ROT : RU_ROT;
            }
            if (ld && rd)
            {
                return biasLeft ? LD_ROT : RD_ROT;
            }

            if (lu && ru)
            {
                return biasLeft ? LU_ROT : RU_ROT;
            }

            if (lu && ld)
            {
                return biasDown ? LD_ROT : LU_ROT;
            }
            if (ru && rd)
            {
                return biasDown ? RD_ROT : RU_ROT;
            }
            
            if (ld)
            {
                return LD_ROT;
            }

            if (lu)
            {
                return LU_ROT;
            }

            if (ru)
            {
                return RU_ROT;
            }

            if (rd)
            {
                return RD_ROT;
            }
            
            if (directionsWithTile.Contains(Direction.Down))
            {
                return biasLeft ? LD_ROT : RD_ROT;
            }

            if (directionsWithTile.Contains(Direction.Up))
            {
                return biasLeft ? LU_ROT : RU_ROT;
            }
            
            if (directionsWithTile.Contains(Direction.Right))
            {
                return biasDown ? RD_ROT : RU_ROT;
            }
            
            if (directionsWithTile.Contains(Direction.Left))
            {
                return biasDown ? LD_ROT : LU_ROT;
            }
            if (biasLeft)
            {
                return biasDown ? LD_ROT : LU_ROT;
            }
            return biasDown ? RD_ROT : RU_ROT;
        }
        private static int CalculateSlabState(HashSet<Direction> directionsWithTile, int mousePlacement)
        {
            const int R_ROT = 1;
            const int D_ROT = 0;
            const int L_ROT = 3;
            const int U_ROT = 2;

            if (directionsWithTile.Count == 4)
            {
                bool biasDown = (mousePlacement & (int)MousePlacement.Down) != 0;
                return biasDown ? D_ROT : U_ROT;
            }
           
            if (directionsWithTile.Contains(Direction.Down) && directionsWithTile.Contains(Direction.Up) && directionsWithTile.Contains(Direction.Left))
            {
                return L_ROT;
            }
            
            if (directionsWithTile.Contains(Direction.Down) && directionsWithTile.Contains(Direction.Up) && directionsWithTile.Contains(Direction.Right))
            {
                return R_ROT;
            }
            
            if (directionsWithTile.Contains(Direction.Down) && directionsWithTile.Contains(Direction.Right) && directionsWithTile.Contains(Direction.Left))
            {
                return D_ROT;
            }
            
            if (directionsWithTile.Contains(Direction.Up) && directionsWithTile.Contains(Direction.Right) && directionsWithTile.Contains(Direction.Left))
            {
                return U_ROT;
            }
            
            if (directionsWithTile.Contains(Direction.Down) && directionsWithTile.Contains(Direction.Up))
            {
                bool biasDown = (mousePlacement & (int)MousePlacement.Down) != 0;
                return biasDown ? D_ROT : U_ROT;
            }
            
            if (directionsWithTile.Contains(Direction.Left) && directionsWithTile.Contains(Direction.Right))
            {
                bool biasLeft = (mousePlacement & (int)MousePlacement.Left) != 0;
                return biasLeft ? L_ROT : R_ROT;
            }
            
            

            
            if (directionsWithTile.Contains(Direction.Up))
            {
                return U_ROT;
            }
            if (directionsWithTile.Contains(Direction.Down))
            {
                return D_ROT;
            }
            
            if (directionsWithTile.Contains(Direction.Right))
            {
                return R_ROT;
            }
            
            if (directionsWithTile.Contains(Direction.Left))
            {
                return L_ROT;
            }
            
            
            return NO_AUTO_ROTATION;
        }

        public static int GetMousePlacement(Vector2 mousePosition)
        {
            int value = 0;
            if (mousePosition.x < TileHelper.getRealTileCenter(mousePosition.x))
            {
                value += (int)MousePlacement.Left;
            }
            else
            {
                value += (int)MousePlacement.Right;
            }

            if (mousePosition.y < TileHelper.getRealTileCenter(mousePosition.y))
            {
                value += (int)MousePlacement.Down;
            }
            else
            {
                value += (int)MousePlacement.Up;
            }

            return value;
        }

        public static bool MouseBiasDirection(int mouseBias, MousePlacement mousePlacement)
        {
            return (mouseBias & (int)mousePlacement) != 0;
        }

        public static bool MouseCentered(bool vertical, Vector2 mousePosition, float epsilon = 0.125f)
        {
            float dif = vertical
                ? mousePosition.x - TileHelper.getRealTileCenter(mousePosition.x)
                : mousePosition.y - TileHelper.getRealTileCenter(mousePosition.y);
            if (dif < 0) dif *= -1;
            return dif <= epsilon;
        }
    }
}
