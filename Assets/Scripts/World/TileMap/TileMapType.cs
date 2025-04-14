using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMaps.Layer;

namespace TileMaps.Type {
    public enum TileMapType {
        Block,
        Background,
        Object,
        Platform,
        ItemConduit,
        FluidConduit,
        EnergyConduit,
        SignalConduit,
        MatrixConduit,
        Fluid
    }

    public static class TileMapTypeExtension {
        public static TileMapLayer ToLayer(this TileMapType type) {
            switch (type) {
                case TileMapType.Block:
                    return TileMapLayer.Base;
                case TileMapType.Background:
                    return TileMapLayer.Background;
                case TileMapType.Object:
                    return TileMapLayer.Base;
                case TileMapType.Platform:
                    return TileMapLayer.Base;
                case TileMapType.ItemConduit:
                    return TileMapLayer.Item;
                case TileMapType.FluidConduit:
                    return TileMapLayer.Fluid;
                case TileMapType.EnergyConduit:
                    return TileMapLayer.Energy;
                case TileMapType.SignalConduit:
                    return TileMapLayer.Signal;
                case TileMapType.MatrixConduit:
                    return TileMapLayer.Matrix;
                case TileMapType.Fluid:
                    return TileMapLayer.Fluid;
                default:
                    Debug.LogError("TileMapTypeExtension method toLayer did not handle switch case for " + type.ToString());
                    return TileMapLayer.Base;
            }
        }
        public static bool HasCollider(this TileMapType tileType) {
            switch (tileType) {
                case TileMapType.Block:
                case TileMapType.Object:
                case TileMapType.Platform:
                case TileMapType.Fluid:
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsTile(this TileMapType tileType) {
            switch (tileType) {
                case TileMapType.Block:
                case TileMapType.Background:
                case TileMapType.Object:
                case TileMapType.Platform:
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsConduit(this TileMapType tileType) {
            switch (tileType) {
                case TileMapType.ItemConduit:
                case TileMapType.FluidConduit:
                case TileMapType.EnergyConduit:
                case TileMapType.SignalConduit:
                case TileMapType.MatrixConduit:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsFluid(this TileMapType tileMapType) {
            return tileMapType == TileMapType.Fluid;
        }

        public static ConduitType ToConduitType(this TileMapType tileMapType) {
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
                    throw new ArgumentOutOfRangeException(nameof(tileMapType), tileMapType, null);
            }
        }
        public static float GetZValue(this TileMapType tileMapType) {
            switch (tileMapType) {
                case TileMapType.Block:
                    return 1;
                case TileMapType.Background:
                    return 3;
                case TileMapType.Object:
                    return 1.5f;
                case TileMapType.Platform:
                    return 1;
                case TileMapType.MatrixConduit:
                    return 2f;
                case TileMapType.ItemConduit:
                    return 2.3f;
                case TileMapType.FluidConduit:
                    return 2.4f;
                case TileMapType.EnergyConduit:
                    return 2.1f;
                case TileMapType.SignalConduit:
                    return 2.2f;
                case TileMapType.Fluid:
                    return 1.1f;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tileMapType), tileMapType, null);
            }
        }
    }
}