using System;
using System.Numerics;
using Tiles;
using UnityEngine;

namespace Conduit.Conduit
{
    public static class ConduitUtils
    {
        public static ConduitDirectionState Reverse(ConduitDirectionState state)
        {
            switch (state)
            {
                case ConduitDirectionState.Up:
                    return ConduitDirectionState.Down;
                case ConduitDirectionState.Down:
                    return ConduitDirectionState.Up;
                case ConduitDirectionState.Left:
                    return ConduitDirectionState.Right;
                case ConduitDirectionState.Right:
                    return ConduitDirectionState.Left;
                case ConduitDirectionState.Active:
                    return ConduitDirectionState.Active;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        
        public static ConduitDirectionState? StateFromVector2Int(Vector2Int direction)
        {
            if (direction == Vector2Int.left)
            {
                return ConduitDirectionState.Left;
            }

            if (direction == Vector2Int.right)
            {
                return ConduitDirectionState.Right;
            }

            if (direction == Vector2Int.up)
            {
                return ConduitDirectionState.Up;
            }

            if (direction == Vector2Int.down)
            {
                return ConduitDirectionState.Down;
            }

            return null;
        }
    
    }
}
