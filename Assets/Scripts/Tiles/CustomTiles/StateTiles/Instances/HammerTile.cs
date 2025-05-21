using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMaps.Place;
using TileMaps.Layer;
using TileMaps.Type;
using TileEntity.Instances.SimonSays;
using Tiles.Fluid.Simulation;
using UnityEngine.Serialization;

namespace Tiles {

    [CreateAssetMenu(fileName = "T~New Hammer Tile", menuName = "Tile/State/Hammer")]
    
    public class HammerTile : TileBase, IStateTileSingle
    {
        public const int BASE_TILE_STATE = 0;
        public const int SLAB_TILE_STATE = 1;
        public const int SLANT_TILE_STATE = 2;
        public const int STAIR_TILE_STATE = 3;
        
        [SerializeField] public TileBase baseTile;
        [SerializeField] public Tile cleanSlab;
        [SerializeField] public Tile cleanSlant;
        [SerializeField] public Tile stairs;
        

        public virtual TileBase GetTileAtState(int state)
        {
            return state switch
            {
                (int)HammerTileState.Solid => baseTile,
                (int)HammerTileState.Slab => cleanSlab,
                (int)HammerTileState.Slant => cleanSlant,
                (int)HammerTileState.Stair => stairs,
                _ => null
            };
        }
        
        public virtual HammerTileState? GetHammerTileState(int state)
        {
            if (state < 4)
            {
                return (HammerTileState)state;
            }
            return null;
        }

        public TileBase GetDefaultTile()
        {
            return baseTile;
        }

        public virtual int GetStateAmount()
        {
            return 4;
        }
        
        public static int GetFlowBitMap(TileItem tileItem, BaseTileData baseTileData)
        {
            const int FLOW_ALL = 15;
            const int FLOW_NONE = 0;
            if (!tileItem) return FLOW_ALL;
            if (tileItem.tileType != TileType.Block) return FLOW_ALL;
            if (tileItem.tile is not HammerTile hammerTile) return FLOW_NONE;
            if (baseTileData == null) return FLOW_ALL;
            int state = baseTileData.state;
            if (state == 0) return FLOW_NONE;
            HammerTileState? optionalState = hammerTile.GetHammerTileState(state);
            if (!optionalState.HasValue) return FLOW_ALL;
            HammerTileState hammerTileState = optionalState.Value;
            int bitMap = GetHammerTileBitMap(hammerTileState);
            int rotation = baseTileData.rotation;
            return RotateFlowBitMap(bitMap, rotation);
        }

        private static int GetHammerTileBitMap(HammerTileState hammerTileState)
        {
            switch (hammerTileState)
            {
                case HammerTileState.Solid:
                    return 0;
                case HammerTileState.Slab:
                    return (int)FluidFlowDirection.Left + (int)FluidFlowDirection.Right + (int)FluidFlowDirection.Up;
                case HammerTileState.Slant:
                case HammerTileState.Stair:
                    return (int)FluidFlowDirection.Left + (int)FluidFlowDirection.Up;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private static int RotateFlowBitMap(int bitMap, int rotation)
        {
            bool CanFlow(FluidFlowDirection direction)
            {
                return (bitMap & (int)direction) != 0;
            }
            
            if (rotation == 0) return bitMap;
            
            int rotatedMap = 0;
            // This is ugly, but no faster and memory efficent way to do it.
            if (CanFlow(FluidFlowDirection.Left))
            {
                switch (rotation)
                {
                   case 1: 
                       rotatedMap += (int)FluidFlowDirection.Down; 
                       break;
                   case 2: 
                       rotatedMap += (int)FluidFlowDirection.Right; 
                       break;
                   case 3: 
                       rotatedMap += (int)FluidFlowDirection.Up; 
                       break;
                }
            }
            if (CanFlow(FluidFlowDirection.Right))
            {
                switch (rotation)
                {
                    case 1: 
                        rotatedMap += (int)FluidFlowDirection.Up; 
                        break;
                    case 2: 
                        rotatedMap += (int)FluidFlowDirection.Left; 
                        break;
                    case 3: 
                        rotatedMap += (int)FluidFlowDirection.Down; 
                        break;
                }
            }
            if (CanFlow(FluidFlowDirection.Down))
            {
                switch (rotation)
                {
                    case 1: 
                        rotatedMap += (int)FluidFlowDirection.Right; 
                        break;
                    case 2: 
                        rotatedMap += (int)FluidFlowDirection.Up; 
                        break;
                    case 3: 
                        rotatedMap += (int)FluidFlowDirection.Left; 
                        break;
                }
            }
            if (CanFlow(FluidFlowDirection.Up))
            {
                switch (rotation)
                {
                    case 1: 
                        rotatedMap += (int)FluidFlowDirection.Left; 
                        break;
                    case 2: 
                        rotatedMap += (int)FluidFlowDirection.Down; 
                        break;
                    case 3: 
                        rotatedMap += (int)FluidFlowDirection.Right; 
                        break;
                }
            }
            return rotatedMap;
        }
    }
    
    public enum HammerTileState
    {
        Solid = 0,
        Slab = 1,
        Slant = 2,
        Stair = 3
    }
}

