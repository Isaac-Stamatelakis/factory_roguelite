using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dimensions;
using TileEntity;
using TileEntity.Instances.CompactMachines;

namespace Chunks.Systems {
    public interface ICompactMachineClosedChunkSystem {
        public CompactMachineTeleportKey GetCompactMachineKey();
        public void SetCompactMachine(CompactMachineInstance compactMachineInstance, CompactMachineTeleportKey key);
        public CompactMachineInstance GetCompactMachine();
    }
    public class CompactMachineClosedChunkSystem : ConduitClosedChunkSystem, ICompactMachineClosedChunkSystem
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

        public void ReSyncCompactMachinePorts(Vector2Int breakCellPosition, ConduitType conduitType, CompactMachinePortType portType)
        {
            foreach (var chunk in cachedChunks.Values)
            {
                foreach (var partition in chunk.GetChunkPartitions())
                {
                    foreach (var compactMachinePort in partition.GetTileEntitiesOfType<ICompactMachineConduitPort>())
                    {
                        Vector2Int cellPosition = compactMachinePort.GetCellPosition();
                        if (cellPosition == breakCellPosition) continue;
                            
                        if (compactMachinePort.GetConduitType() != conduitType || compactMachinePort.GetPortType() != portType) continue;
                        compactMachinePort.SyncToCompactMachine(compactMachineInstance);
                    }
                    
                }
            }
        }
    }
}

