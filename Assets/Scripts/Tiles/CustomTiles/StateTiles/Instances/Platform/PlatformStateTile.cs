using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.CustomTiles.StateTiles.Instances.Platform
{
 
    public enum PlatformTileState
    {
        FlatConnectNone,
        FlatConnectOne,
        FlatConnectAll,
        Slope,
        FlatSlopeConnectOne,
        FlatSlopeConnectAll,
    }
    public class PlatformStateTile : TileBase, IStateTileMultiple
    {
        public TileBase FlatConnectNone;
        public TileBase FlatConnectOne;
        public TileBase FlatConnectAll;
        public TileBase Slope;
        public TileBase SlopeDeco;
        
        
        public TileBase GetDefaultTile()
        {
            return FlatConnectAll;
        }

        public int GetStateAmount()
        {
            return Enum.GetNames(typeof(PlatformTileState)).Length;
        }

        public void GetTiles(int state, TileBase[] container)
        {
            PlatformTileState platformState = (PlatformTileState)state;
            switch (platformState)
            {
                case PlatformTileState.FlatConnectNone:
                    container[0] = FlatConnectNone;
                    container[1] = null;
                    container[2] = null;
                    break;
                case PlatformTileState.FlatConnectOne:
                    container[0] = FlatConnectOne;
                    container[1] = null;
                    container[2] = null;
                    break;
                case PlatformTileState.FlatConnectAll:
                    container[0] = FlatConnectAll;
                    container[1] = null;
                    container[2] = null;
                    break;
                case PlatformTileState.Slope:
                    container[0] = null;
                    container[1] = Slope;
                    container[2] = SlopeDeco;
                    break;
                case PlatformTileState.FlatSlopeConnectOne:
                    container[0] = FlatConnectOne;
                    container[1] = Slope;
                    container[2] = SlopeDeco;
                    break;
                case PlatformTileState.FlatSlopeConnectAll:
                    container[0] = FlatConnectAll;
                    container[1] = Slope;
                    container[2] = SlopeDeco;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
