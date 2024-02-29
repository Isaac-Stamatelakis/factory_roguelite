using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldModule.Generation;

namespace TileEntityModule.Instances {
    public enum CaveRegion
    {
        Cave0,
        Cave1,
        Cave2,
        Cave3,
        Cave4,
        Cave5
    }

    public static class CaveRegionExtension {
        public static string getDescription(this CaveRegion caveRegion){
            switch (caveRegion) {
                case CaveRegion.Cave0:
                    return "Cave 0";
                case CaveRegion.Cave1:
                    return "Cave 1";
                case CaveRegion.Cave2:
                    return "Cave 2";
                case CaveRegion.Cave3:
                    return "Cave 3";
                case CaveRegion.Cave4:
                    return "Cave 4";
                case CaveRegion.Cave5:
                    return "Cave 5";
            }
            return "";
        }
        public static GeneratedArea getGeneratedArea(this CaveRegion caveRegion) {
            switch (caveRegion) {
                case CaveRegion.Cave0:
                    return Resources.Load<GeneratedArea>("Areas/Area1/Area1");
                case CaveRegion.Cave1:
                    return Resources.Load<GeneratedArea>("Areas/Area2/Area2");
                case CaveRegion.Cave2:
                    return null;
                case CaveRegion.Cave3:
                    return null;
                case CaveRegion.Cave4:
                    return null;
                case CaveRegion.Cave5:
                    return null;
            }
            return null;
        }
    }
    public static class CaveRegionFactory {
        public static List<CaveRegion> getRegionsUnlocked(int stage) {
            List<CaveRegion> caves = new List<CaveRegion>();
            if (stage >= 0) {
                caves.Add(CaveRegion.Cave0);
            }
            if (stage >= 1) {
                caves.Add(CaveRegion.Cave1);
                caves.Add(CaveRegion.Cave2);
            }
            if (stage >= 2) {
                caves.Add(CaveRegion.Cave3);
                caves.Add(CaveRegion.Cave4);
                caves.Add(CaveRegion.Cave5);
            }
            return caves;
        }
    }
    
}

