using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dimensions;
using TileEntity.Instances.CompactMachines;

namespace Chunks.Systems {
    public interface ICompactMachineClosedChunkSystem {
        public CompactMachineTeleportKey GetCompactMachineKey();
        public void SetCompactMachine(CompactMachineInstance compactMachineInstance, CompactMachineTeleportKey key);
        public CompactMachineInstance GetCompactMachine();
    }
    public class CompactMachineClosedChunkSystem : ConduitTileClosedChunkSystem, ICompactMachineClosedChunkSystem
    {
        private CompactMachineTeleportKey compactMachinePosition;
        private CompactMachineInstance compactMachineInstance;
        public CompactMachineTeleportKey GetCompactMachineKey()
        {
            return compactMachinePosition;
        }

        public void SetCompactMachine(CompactMachineInstance compactMachineInstance, CompactMachineTeleportKey key)
        {
            this.compactMachineInstance = compactMachineInstance;
            compactMachinePosition = key;
            interactable = !key.Locked;
        }

        public CompactMachineInstance GetCompactMachine()
        {
            return compactMachineInstance;
        }
    }
}

