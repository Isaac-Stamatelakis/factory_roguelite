using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntity{
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

    public static class TierUtils {
        public static ulong GetEnergyStorage(this Tier tier) {
            return GetMaxEnergyUsage(tier) << 10; // Times 1024
        }
        public static ulong GetMaxEnergyUsage(this Tier tier)
        {
            const ulong baseEnergy = 32;
            return baseEnergy << (2*(int)tier);
        }
        public static uint GetFluidStorage(this Tier tier) {
            const uint baseStorage = 8192;
            return baseStorage << (2*(int)tier);
        }
        
    }
}
