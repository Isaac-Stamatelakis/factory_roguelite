using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntity{
    public enum Tier {
        [InspectorName("No Minimum (Untiered)")] Untiered = -1,
        Basic = 0,
        Advanced = 1,
        Elite = 2,
        Master = 3,
        Ultimate = 4,
        Paramount = 5,
        Stellar = 6,
        Transcendent = 7,
        Zenith = 8,
        Infinity = 9,
        [InspectorName("No Maximum (Disabled)")]  Disabled = 99
    }

    public static class TierUtils {
        public static ulong GetEnergyStorage(this Tier tier) {
            return GetMaxEnergyUsage(tier) << 10; // Times 1024
        }
        public static ulong GetMaxEnergyUsage(this Tier tier)
        {
            const ulong baseEnergy = 8;
            return baseEnergy << (2*((int)tier+1));
        }
        public static uint GetFluidStorage(this Tier tier) {
            const uint baseStorage = 8000;
            return baseStorage << (2*((int)tier+1));
        }
        
    }
}
