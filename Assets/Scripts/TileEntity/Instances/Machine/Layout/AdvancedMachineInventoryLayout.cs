using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items.Inventory;

namespace TileEntityModule.Instances.Machines {
    [CreateAssetMenu(fileName = "New Machine Layout", menuName = "Tile Entity/Machine/Layout/Advanced")]
    public class AdvancedMachineInventoryLayout : InventoryLayout {
        public List<AdvancedInventoryLayout> inputInventories;
        public List<AdvancedInventoryLayout> outputInventories;
    }
}

