using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles
{
    public class ConduitStateTile : TileBase, IStateTile, IIDTile
    {
        public string Id;
        public Tile[] Tiles;
        
        public TileBase getTileAtState(int state)
        {
            if (state >= Tiles.Length || state < 0)
            {
                Debug.LogWarning($"Invalid state for conduit tile {state} {name}");
            }
            return Tiles[state];
        }

        public Sprite getDefaultSprite()
        {
            const int allDirectionState =   (int)ConduitDirectionState.Left
                                          + (int)ConduitDirectionState.Right
                                          + (int)ConduitDirectionState.Up
                                          + (int)ConduitDirectionState.Down;
            return Tiles[allDirectionState].sprite;
        }

        public int getStateAmount()
        {
            return Tiles.Length;
        }

        public string getId()
        {
            return Id;
        }

        public void setID(string id)
        {
            this.Id = id;
        }
    }
}

