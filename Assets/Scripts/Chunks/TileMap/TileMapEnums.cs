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

public enum TileMapLayer {
    Base,
    Background,
    Item,
    Fluid,
    Energy,
    Signal,
    Null,
}

public class TileMapTypeBundle {
    public int z;
    public LayerMask layerMask;
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
    private static Dictionary<TileMapLayer, List<TileMapType>> layerToTileMapTypes;
    private static readonly Dictionary<TileMapType, TileMapLayer> tileMapTypeToSerializeLayer = new Dictionary<TileMapType, TileMapLayer> {
        { TileMapType.Block, TileMapLayer.Base },
        { TileMapType.Background, TileMapLayer.Background },
        { TileMapType.Object, TileMapLayer.Base },
        { TileMapType.SlipperyBlock, TileMapLayer.Base },
        { TileMapType.ClimableObject, TileMapLayer.Base },
        { TileMapType.Platform, TileMapLayer.Base },
        { TileMapType.ColladableObject, TileMapLayer.Base },
        { TileMapType.ItemConduit, TileMapLayer.Item },
        { TileMapType.FluidConduit, TileMapLayer.Fluid},
        { TileMapType.EnergyConduit, TileMapLayer.Energy},
        { TileMapType.SignalConduit, TileMapLayer.Signal}
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

    public static TileMapLayer MapToSerializeLayer(TileMapType type) {
        return tileMapTypeToSerializeLayer.TryGetValue(type, out var serializeLayer) ? serializeLayer : TileMapLayer.Null;
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

    public static List<TileMapType> getTileTypesInLayer(TileMapLayer layer) {
        if (layerToTileMapTypes == null) {
            initLayerToTIleMapType();
        }
        return layerToTileMapTypes[layer];
    }

    protected static void initLayerToTIleMapType() {
        layerToTileMapTypes = new Dictionary<TileMapLayer, List<TileMapType>>();
        foreach (var keyValuePair in tileMapTypeToSerializeLayer){
            TileMapLayer dictLayer = keyValuePair.Value;
            TileMapType dictType = keyValuePair.Key;

            if (!layerToTileMapTypes.ContainsKey(dictLayer))
            {
                layerToTileMapTypes[dictLayer] = new List<TileMapType>();
            }
            layerToTileMapTypes[dictLayer].Add(dictType);
        }
    }
    public static List<LayerMask> getLayerMasksInLayer(TileMapLayer layer) {
        if (layerToTileMapTypes == null) {
            initLayerToTIleMapType();
        }
        List<TileMapType> tileMapTypes = layerToTileMapTypes[layer];
        List<LayerMask> layers = new List<LayerMask>();
        foreach (TileMapType tileMapType in tileMapTypes) {
            layers.Add(1 << LayerMask.NameToLayer(tileMapType.ToString()));
        }
        return layers;
    }
    public static float getTileMapZValue(TileMapType tileMapType) {
        switch (tileMapType) {
            case TileMapType.Block:
                return 1;
            case TileMapType.Background:
                return 3;
            case TileMapType.Object:
                return 1;
            case TileMapType.Platform:
                return 1;
            case TileMapType.SlipperyBlock:
                return 1;
            case TileMapType.ColladableObject:
                return 1;
            case TileMapType.ClimableObject:
                return 1;
            case TileMapType.ItemConduit:
                return 4;
            case TileMapType.FluidConduit:
                return 3;
            case TileMapType.EnergyConduit:
                return 2;
            case TileMapType.SignalConduit:
                return 1;
            default:
                return -9999;
        }
    }

}