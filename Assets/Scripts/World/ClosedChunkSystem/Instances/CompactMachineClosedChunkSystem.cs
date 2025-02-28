using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dimensions;

namespace Chunks.Systems {
    public interface ICompactMachineClosedChunkSystem {
        public CompactMachineTeleportKey getCompactMachineKey();
        public void setCompactMachineKey(CompactMachineTeleportKey path);
    }
    public class CompactMachineClosedChunkSystem : ConduitTileClosedChunkSystem, ICompactMachineClosedChunkSystem
    {
        private CompactMachineTeleportKey compactMachinePosition;
        public CompactMachineTeleportKey getCompactMachineKey()
        {
            return compactMachinePosition;
        }

        public void setCompactMachineKey(CompactMachineTeleportKey position)
        {
            compactMachinePosition = position;
        }
    }
}

