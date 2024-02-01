using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileMapType {
    Block = TileType.Block,
    Background = TileType.Background,
    Object = TileType.Object,
    ItemConduit = ConduitType.Item,
    FluidConduit = ConduitType.Fluid,
    EnergyConduit = ConduitType.Energy,
    SignalConduit = ConduitType.Signal
}

public class TileMapTypeFactory {
    protected static Dictionary<TileType, TileMapType> tilesToMap = new Dictionary<TileType, TileMapType>{
        {TileType.Block, TileMapType.Block},
        {TileType.Background, TileMapType.Background},
        {TileType.Object, TileMapType.Object},
    };

    protected static Dictionary<ConduitType, TileMapType> conduitsToMap = new Dictionary<ConduitType, TileMapType>{
        {ConduitType.Item, TileMapType.ItemConduit},
        {ConduitType.Fluid, TileMapType.FluidConduit},
        {ConduitType.Energy, TileMapType.EnergyConduit},
        {ConduitType.Signal, TileMapType.SignalConduit},
    };

    public static TileMapType tileToMapType(TileType tileType) {
        return tilesToMap[tileType];
    }
    public static TileMapType tileToMapType(ConduitType conduitType) {
        return conduitsToMap[conduitType];
    }
}