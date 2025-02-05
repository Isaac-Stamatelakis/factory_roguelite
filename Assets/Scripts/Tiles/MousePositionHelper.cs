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
        public static int CalculateHammerTileRotation(Vector2 mousePosition, int placementState)
        {
            const int NO_AUTO_ROTATION = -1;
            if (placementState == 0) return NO_AUTO_ROTATION;
            List<Direction> directions = new List<Direction>
                { Direction.Left, Direction.Right, Direction.Down, Direction.Up };
            HashSet<Direction> directionsWithTile = new HashSet<Direction>();

            foreach (Direction direction in directions)
            {
                 if (PlaceTile.tileInDirection(mousePosition, direction, TileMapLayer.Base)) directionsWithTile.Add(direction);
            }
            
            int mousePlacement = GetMousePlacement(mousePosition);
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

            if (ru) return RU_ROT;
            if (rd) return RD_ROT;
            if (lu) return LU_ROT;
            if (ld) return LD_ROT;
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
