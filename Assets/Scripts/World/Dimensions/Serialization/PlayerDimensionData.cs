using System;
using System.Collections.Generic;
using Dimensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Player;
using UnityEngine;

namespace World.Dimensions.Serialization
{
    public class DimensionData
    {
        public int Dim;
        public string DimData;

        public DimensionData(int dim, string dimData)
        {
            Dim = dim;
            DimData = dimData;
        }
    }
    public class PlayerDimensionData
    {
        public DimensionData DimensionData;
        public float X;
        public float Y;

        public PlayerDimensionData(DimensionData dimensionData, float x, float y)
        {
            DimensionData = dimensionData;
            X = x;
            Y = y;
        }
    }

    public static class PlayerDimensionDataFactory
    {
        public static DimensionData SerializeDimensionData(DimensionManager dimensionManager)
        {
            Dimension? dimension = dimensionManager.GetPlayerDimensionType();
            if (!dimension.HasValue) return null;
            string dimData = GetDimensionData(dimension.Value, dimensionManager);
            return new DimensionData((int)dimension, dimData);
        }
        private static string GetDimensionData(Dimension dimension, DimensionManager dimensionManager)
        {
            switch (dimension)
            {
                case Dimension.OverWorld:
                    return null;
                case Dimension.Cave:
                    CaveController caveController = (CaveController)dimensionManager.GetDimController(Dimension.Cave);
                    return caveController.GetCurrentCaveId();
                case Dimension.CompactMachine:
                    CompactMachineDimController compactMachineDimController = (CompactMachineDimController)dimensionManager.GetDimController(Dimension.CompactMachine);
                    List<Vector2Int> currentPath = compactMachineDimController.CurrentSystemPath;
                    bool locked = compactMachineDimController.IsLocked(currentPath);
                    return JsonConvert.SerializeObject(new CompactMachineTeleportKey(currentPath,locked));
                default:
                    throw new ArgumentOutOfRangeException(nameof(dimension), dimension, null);
            }
        }
    }
}
