using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntityModule{
    public enum Tier {
        Basic,
        Advanced,
        Elite,
        Master,
        Ultimate,
        Paramount,
        Stellar,
        Transcendent,
        Zenith,
        Infinity
    }

    public static class TierExtension {
        public static int getEnergyStorage(this Tier tier) {
            return 1024*getMaxEnergyUsage(tier);
        }
        public static int getMaxEnergyUsage(this Tier tier) {
            return (int) Mathf.Pow(4, (int) tier);
        }
        public static int getFluidStorage(this Tier tier) {
            return (int) Mathf.Pow(2,(int) tier)*8000;
        }
    }
}
