using System.Collections;
using System.Collections.Generic;
using UI.NodeNetwork;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles
{
    public class ConduitStateTile : TileBase, IStateTileSingle, INoDelayPreviewTile
    {
        public Tile[] Tiles;
        
        public TileBase GetTileAtState(int state)
        {
            if (state < Tiles.Length && state >= 0) return Tiles[state];
            Debug.LogWarning($"Invalid state for conduit tile {state} {name}");
            return null;
        }

        public TileBase GetDefaultTile()
        {
            const int ALL_DIRECTION_STATE =   (int)ConduitDirectionState.Left
                                            + (int)ConduitDirectionState.Right
                                            + (int)ConduitDirectionState.Up
                                            + (int)ConduitDirectionState.Down;
            return Tiles[ALL_DIRECTION_STATE];
        }

        public int GetStateAmount()
        {
            return Tiles.Length;
        }
    }
}

