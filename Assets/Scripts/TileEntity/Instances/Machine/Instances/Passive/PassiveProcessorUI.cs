using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TileEntityModule.Instances.Machines {
    public class PassiveProcessorUI : MonoBehaviour
    {
        [SerializeField] public TextMeshProUGUI title;
        [SerializeField] public ArrowProgressController arrowProgressController;
        private GameObject slotPrefab;
        private Tier tier;
        private PassiveProcessorInventory machineInventory;
        public void displayMachine(MachineInventoryLayout layout, PassiveProcessorInventory machineInventory, string machineName, Tier tier) {
            machineInventory.display(layout,transform);
            this.machineInventory = machineInventory;
            this.tier = tier;
            title.text = MachineUIFactory.formatMachineName(machineName);
        }

        private void setArrow() {
            //arrowProgressController.setArrow()
        }
    }
}
