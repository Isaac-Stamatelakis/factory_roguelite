using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapModule.Type;

namespace ChunkModule.ClosedChunkSystemModule {
    public static class TileMapBundleFactory 
    {
        public static List<TileMapType> getStandardTileTypes() {
            return new List<TileMapType>{
                TileMapType.Block,
                TileMapType.Background,
                TileMapType.Object,
                TileMapType.Platform,
                TileMapType.SlipperyBlock,
                TileMapType.ColladableObject,
                TileMapType.ClimableObject
            };
        }
        public static List<TileMapType> getConduitTileTypes() {
            return new List<TileMapType>{
                TileMapType.ItemConduit,
                TileMapType.FluidConduit,
                TileMapType.SignalConduit,
                TileMapType.EnergyConduit,
            };
        }
    }
}

