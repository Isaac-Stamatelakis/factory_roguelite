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

        public void onRightClick()
        {
            GameObject uiPrefab = TileEntityObject.UIManager.getUIElement();
            if (uiPrefab == null) {
                CompactMachineHelper.teleportOutOfCompactMachine(compactMachine);
                return;
            }
            // Not null show ui prefab
            /*
            GameObject instantiated = GameObject.Instantiate(uiPrefab);
            CompactMachineUIController uIController = instantiated.GetComponent<CompactMachineUIController>();
            if (uIController == null) {
                Debug.LogError(name + "ui prefab doesn't have controller");
                return;
            }
            uIController.display(this);
            GlobalUIContainer.getInstance().getUiController().setGUI(instantiated);
            */
        }

        public void syncToCompactMachine(CompactMachineInstance compactMachine)
        {
            this.compactMachine = compactMachine;
            compactMachine.Teleporter = this;
        }
    }
}

