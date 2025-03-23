using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntity.Instances.CompactMachines {
    public class CompactMachineTeleporterInstance : TileEntityInstance<CompactMachineTeleporter>, ICompactMachineInteractable, IRightClickableTileEntity
    {
        private CompactMachineInstance compactMachine;
        public CompactMachineTeleporterInstance(CompactMachineTeleporter tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
        
        public void SyncToCompactMachine(CompactMachineInstance compactMachine)
        {
            this.compactMachine = compactMachine;
            compactMachine.Teleporter = this;
        }
        
        public void OnRightClick()
        {
            CompactMachineUtils.TeleportOutOfCompactMachine(compactMachine);
        }
    }
}

