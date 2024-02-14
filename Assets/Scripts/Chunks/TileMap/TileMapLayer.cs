using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapModule.Type;

namespace TileMapModule.Layer {
    public enum TileMapLayer {
        Base,
        Background,
        Item,
        Fluid,
        Energy,
        Signal,
    }

    public static class TileMapLayerExtension {
        public static List<TileMapType> getTileMapTypes(this TileMapLayer layer) {
            switch (layer) {
                case TileMapLayer.Base:
                    return new List<TileMapType>{
                        TileMapType.Block,
                        TileMapType.ClimableObject,
                        TileMapType.Platform,
                        TileMapType.Object,
                        TileMapType.SlipperyBlock,
                        TileMapType.ColladableObject
                    };
                case TileMapLayer.Background:
                    return new List<TileMapType>{
                        TileMapType.Background
                    };
                case TileMapLayer.Item:
                    return new List<TileMapType>{
                        TileMapType.ItemConduit
                    };
                case TileMapLayer.Fluid:
                    return new List<TileMapType>{
                        TileMapType.FluidConduit
                    };
                case TileMapLayer.Energy:
                    return new List<TileMapType>{
                        TileMapType.EnergyConduit
                    };
                case TileMapLayer.Signal:
                    return new List<TileMapType>{
                        TileMapType.SignalConduit
                    };
                default:
                    return null;
            }
        }
        public static bool raycastable(this TileMapLayer layer) {
            switch (layer) {
                case TileMapLayer.Base:
                    return true;
                case TileMapLayer.Background:
                    return true;
                case TileMapLayer.Item:
                    return false;
                case TileMapLayer.Fluid:
                    return false;
                case TileMapLayer.Energy:
                    return false;
                case TileMapLayer.Signal:
                    return false;
                default:
                    return false;
            }
        }
        public static int toRaycastLayers(this TileMapLayer layer) {
            List<TileMapType> tileMapTypes = layer.getTileMapTypes();
            if (tileMapTypes == null) {
                return 0;
            }
            int layerMask = 0;
            foreach (TileMapType tileMapType in tileMapTypes) {
                layerMask |= (1 << LayerMask.NameToLayer(tileMapType.ToString()));
            }
            return layerMask;
        }
    }
}