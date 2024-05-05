using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapModule.Layer;

namespace TileMapModule.Type {
    public enum TileMapType {
        Block,
        Background,
        Object,
        Platform,
        SlipperyBlock,
        ColladableObject,
        ItemConduit,
        FluidConduit,
        EnergyConduit,
        SignalConduit,
        MatrixConduit,
        Fluid
    }

    public static class TileMapTypeExtension {
        public static TileMapLayer toLayer(this TileMapType type) {
            switch (type) {
                case TileMapType.Block:
                    return TileMapLayer.Base;
                case TileMapType.Background:
                    return TileMapLayer.Background;
                case TileMapType.Object:
                    return TileMapLayer.Base;
                case TileMapType.Platform:
                    return TileMapLayer.Base;
                case TileMapType.SlipperyBlock:
                    return TileMapLayer.Base;
                case TileMapType.ColladableObject:
                    return TileMapLayer.Base;
                case TileMapType.ItemConduit:
                    return TileMapLayer.Item;
                case TileMapType.FluidConduit:
                    return TileMapLayer.Fluid;
                case TileMapType.EnergyConduit:
                    return TileMapLayer.Energy;
                case TileMapType.SignalConduit:
                    return TileMapLayer.Signal;
                default:
                    Debug.LogError("TileMapTypeExtension method toLayer did not handle switch case for " + type.ToString());
                    return TileMapLayer.Base;
            }
        }
        public static bool hasCollider(this TileMapType tileType) {
            switch (tileType) {
                case TileMapType.Block:
                    return true;
                case TileMapType.Background:
                    return true;
                case TileMapType.Object:
                    return true;
                case TileMapType.Platform:
                    return true;
                case TileMapType.SlipperyBlock:
                    return true;
                case TileMapType.ColladableObject:
                    return true;
                case TileMapType.ItemConduit:
                    return false;
                case TileMapType.FluidConduit:
                    return false;
                case TileMapType.EnergyConduit:
                    return false;
                case TileMapType.SignalConduit:
                    return false;
                default:
                    return false;
            }
        }
        public static bool isTile(this TileMapType tileType) {
            switch (tileType) {
                case TileMapType.Block:
                    return true;
                case TileMapType.Background:
                    return true;
                case TileMapType.Object:
                    return true;
                case TileMapType.Platform:
                    return true;
                case TileMapType.SlipperyBlock:
                    return true;
                case TileMapType.ColladableObject:
                    return true;
                case TileMapType.ItemConduit:
                    return false;
                case TileMapType.FluidConduit:
                    return false;
                case TileMapType.EnergyConduit:
                    return false;
                case TileMapType.SignalConduit:
                    return false;
                default:
                    return false;
            }
        }
        public static bool isConduit(this TileMapType tileType) {
            switch (tileType) {
                case TileMapType.Block:
                    return false;
                case TileMapType.Background:
                    return false;
                case TileMapType.Object:
                    return false;
                case TileMapType.Platform:
                    return false;
                case TileMapType.SlipperyBlock:
                    return false;
                case TileMapType.ColladableObject:
                    return false;
                case TileMapType.ItemConduit:
                    return true;
                case TileMapType.FluidConduit:
                    return true;
                case TileMapType.EnergyConduit:
                    return true;
                case TileMapType.SignalConduit:
                    return true;
                case TileMapType.MatrixConduit:
                    return true;
                default:
                    return false;
            }
        }

        public static bool isFluid(this TileMapType tileMapType) {
            return tileMapType == TileMapType.Fluid;
        }

        public static ConduitType toConduitType(this TileMapType tileMapType) {
            switch (tileMapType) {
                case TileMapType.ItemConduit:
                    return ConduitType.Item;
                case TileMapType.FluidConduit:
                    return ConduitType.Fluid;
                case TileMapType.EnergyConduit:
                    return ConduitType.Energy;
                case TileMapType.SignalConduit:
                    return ConduitType.Signal;
                case TileMapType.MatrixConduit:
                    return ConduitType.Matrix;
                default:
                    Debug.LogError("Invalid tilemap type provided for converting to conduitytpe");
                    return ConduitType.Item;
            }
        }
        public static float getZValue(this TileMapType tileMapType) {
            switch (tileMapType) {
                case TileMapType.Block:
                    return 1;
                case TileMapType.Background:
                    return 3;
                case TileMapType.Object:
                    return 1.5f;
                case TileMapType.Platform:
                    return 1;
                case TileMapType.SlipperyBlock:
                    return 1;
                case TileMapType.ColladableObject:
                    return 1;
                case TileMapType.MatrixConduit:
                    return 2f;
                case TileMapType.ItemConduit:
                    return 2.1f;
                case TileMapType.FluidConduit:
                    return 2.2f;
                case TileMapType.EnergyConduit:
                    return 2.3f;
                case TileMapType.SignalConduit:
                    return 2.4f;
                case TileMapType.Fluid:
                    return 1.1f;
                default:
                    return 9999;
            }
        }
    }
}