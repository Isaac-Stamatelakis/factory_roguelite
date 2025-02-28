using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dimensions;

namespace Chunks.Systems {
    public interface ICompactMachineClosedChunkSystem {
        public CompactMachineTeleportKey GetCompactMachineKey();
        public void setCompactMachineKey(CompactMachineTeleportKey path);
    }
    public class CompactMachineClosedChunkSystem : ConduitTileClosedChunkSystem, ICompactMachineClosedChunkSystem
    {
        private CompactMachineTeleportKey compactMachinePosition;
        public CompactMachineTeleportKey GetCompactMachineKey()
        {
            return compactMachinePosition;
        }

        public void setCompactMachineKey(CompactMachineTeleportKey key)
        {
            compactMachinePosition = key;
            interactable = key.Locked;
        }
    }
}

