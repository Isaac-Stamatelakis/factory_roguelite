using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.CustomTiles.StateTiles.Instances.Platform
{
    public interface IRestrictedIndicatorStateTile : INamedStateTile
    {
        public int ShiftState(int currentState, int dir);
    }
    
    public enum PlatformTileState
    {
        FlatConnectNone,
        FlatConnectOne,
        FlatConnectAll,
        Slope,
        FlatSlopeConnectOne,
        FlatSlopeConnectAll,
    }
    public class PlatformStateTile : TileBase, IStateTileMultiple, INamedStateTile, IRestrictedIndicatorStateTile
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

        public string GetStateName(int state)
        {
            PlatformTileState platformTileState =  (PlatformTileState)state;
            switch (platformTileState)
            {
                case PlatformTileState.FlatConnectNone:
                case PlatformTileState.FlatConnectOne:
                case PlatformTileState.FlatConnectAll:
                    return "Flat Platform";
                case PlatformTileState.Slope:
                    return "Sloped Platform";
                case PlatformTileState.FlatSlopeConnectOne:
                case PlatformTileState.FlatSlopeConnectAll:
                    return "Flat & Sloped Platform";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public int ShiftState(int currentState, int dir)
        {
            PlatformTileState platformTileState =  (PlatformTileState)currentState;
            switch (platformTileState)
            {
                case PlatformTileState.FlatConnectNone:
                case PlatformTileState.FlatConnectOne:
                case PlatformTileState.FlatConnectAll:
                case PlatformTileState.FlatSlopeConnectOne:
                case PlatformTileState.FlatSlopeConnectAll:
                    return (int)PlatformTileState.Slope;
                case PlatformTileState.Slope:
                    return (int)PlatformTileState.FlatConnectNone;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
