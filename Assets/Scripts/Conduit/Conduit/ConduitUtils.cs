using System;
using Tiles;

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
    
    }
}
