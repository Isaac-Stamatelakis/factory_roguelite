using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntity.Instances.CompactMachines {
    public class CompactMachineTeleporterInstance : TileEntityInstance<CompactMachineTeleporter>, IRightClickableTileEntity, ICompactMachineInteractable
    {
        private CompactMachineInstance compactMachine;
        public CompactMachineTeleporterInstance(CompactMachineTeleporter tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public void OnRightClick()
        {
            GameObject uiPrefab = TileEntityObject.UIManager.getUIElement();
            if (compactMachine == null)
            {
                Debug.LogWarning("Tried to teleport out of compact machine with unsynced teleporter");
                return;
            }
            CompactMachineUtils.TeleportOutOfCompactMachine(compactMachine);
        }

        public void SyncToCompactMachine(CompactMachineInstance compactMachine)
        {
            this.compactMachine = compactMachine;
            compactMachine.Teleporter = this;
        }
    }
}

