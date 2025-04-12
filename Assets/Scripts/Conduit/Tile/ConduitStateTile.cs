using System.Collections;
using System.Collections.Generic;
using UI.NodeNetwork;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles
{
    public class ConduitStateTile : TileBase, IStateTile, INoDelayPreviewTile
    {
        public Tile[] Tiles;
        
        public TileBase getTileAtState(int state)
        {
            if (state < Tiles.Length && state >= 0) return Tiles[state];
            Debug.LogWarning($"Invalid state for conduit tile {state} {name}");
            return null;
        }

        public TileBase GetDefaultTile()
        {
            const int allDirectionState =   (int)ConduitDirectionState.Left
                                            + (int)ConduitDirectionState.Right
                                            + (int)ConduitDirectionState.Up
                                            + (int)ConduitDirectionState.Down;
            return Tiles[allDirectionState];
        }

        public int getStateAmount()
        {
            return Tiles.Length;
        }
    }
}

