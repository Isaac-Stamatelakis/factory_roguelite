using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileMapType {
    Block,
    Background,
    Object,
    Platform,
    SlipperyBlock,
    ColladableObject,
    ClimableObject,
    ItemConduit,
    FluidConduit,
    EnergyConduit,
    SignalConduit,
}

public enum SerializeLayer {
    Base,
    Background,
    Item,
    Fluid,
    Energy,
    Signal,
    Null,
}

public class TileMapTypeFactory {
    protected static readonly Dictionary<TileType, TileMapType> tilesToMap = new Dictionary<TileType, TileMapType>{
        {TileType.Block, TileMapType.Block},
        {TileType.Background, TileMapType.Background},
        {TileType.Object, TileMapType.Object},
        { TileType.SlipperyBlock, TileMapType.SlipperyBlock},
        { TileType.ClimableObject, TileMapType.ClimableObject},
        { TileType.Platform, TileMapType.Platform},
        { TileType.ColladableObject, TileMapType.ColladableObject},
    };

    protected static readonly Dictionary<ConduitType, TileMapType> conduitsToMap = new Dictionary<ConduitType, TileMapType>{
        {ConduitType.Item, TileMapType.ItemConduit},
        {ConduitType.Fluid, TileMapType.FluidConduit},
        {ConduitType.Energy, TileMapType.EnergyConduit},
        {ConduitType.Signal, TileMapType.SignalConduit},
    };
    
    private static readonly Dictionary<TileMapType, SerializeLayer> tileMapTypeToSerializeLayer = new Dictionary<TileMapType, SerializeLayer> {
        { TileMapType.Block, SerializeLayer.Base },
        { TileMapType.Background, SerializeLayer.Background },
        { TileMapType.Object, SerializeLayer.Base },
        { TileMapType.SlipperyBlock, SerializeLayer.Base },
        { TileMapType.ClimableObject, SerializeLayer.Base },
        { TileMapType.Platform, SerializeLayer.Base },
        { TileMapType.ColladableObject, SerializeLayer.Base },
        { TileMapType.ItemConduit, SerializeLayer.Item },
        { TileMapType.FluidConduit, SerializeLayer.Fluid},
        { TileMapType.EnergyConduit, SerializeLayer.Energy},
        { TileMapType.SignalConduit, SerializeLayer.Signal}
    };
    private static readonly HashSet<TileMapType> tileTypeSet = new HashSet<TileMapType>
    {
        TileMapType.Block,
        TileMapType.Background,
        TileMapType.Object,
        TileMapType.Platform,
        TileMapType.SlipperyBlock,
        TileMapType.ColladableObject,
        TileMapType.ClimableObject
    };
    private static readonly HashSet<TileMapType> conduitTypeSet = new HashSet<TileMapType>
    {
        TileMapType.ItemConduit,
        TileMapType.FluidConduit,
        TileMapType.EnergyConduit,
        TileMapType.SignalConduit
    };

    public static SerializeLayer MapToSerializeLayer(TileMapType type) {
        return tileMapTypeToSerializeLayer.TryGetValue(type, out var serializeLayer) ? serializeLayer : SerializeLayer.Null;
    }
    public static TileMapType tileToMapType(TileType tileType) {
        return tilesToMap[tileType];
    }
    public static TileMapType tileToMapType(ConduitType conduitType) {
        return conduitsToMap[conduitType];
    }

    public static bool typeIsTile(TileMapType tileType) {
        return tileTypeSet.Contains(tileType);
    }
    public static bool typeIsConduit(TileMapType tileType) {
        return conduitTypeSet.Contains(tileType);
    }
}