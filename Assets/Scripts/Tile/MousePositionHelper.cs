using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tiles {
    public enum VerticalMousePosition {
        Bottom,
        Top
        }
    public enum HorizontalMousePosition {
        Left,
        Right
    }
    public static class MousePositionFactory {
        public static VerticalMousePosition getVerticalMousePosition(Vector2 position) {
            if (isInBottomHalf(position)) {
                return VerticalMousePosition.Bottom;
            }
            return VerticalMousePosition.Top;
        }

        private static bool isInBottomHalf(Vector2 position) { 
            return position.y % 0.5f < 0.25f;
        }
        public static HorizontalMousePosition getHorizontalMousePosition(Vector2 position) {
            if (isOnLeft(position)) {
                return HorizontalMousePosition.Left;
            }
            return HorizontalMousePosition.Right;
        }

        private static bool isOnLeft(Vector2 position) {
            return position.x % 0.5f < 0.25f;
        }
    }

}
