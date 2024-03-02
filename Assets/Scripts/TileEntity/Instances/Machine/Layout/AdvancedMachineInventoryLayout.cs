using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TileEntityModule.Instances.Machines {
    [CreateAssetMenu(fileName = "New Machine Layout", menuName = "Tile Entity/Machine/Layout/Advanced")]
    public class AdvancedMachineInventoryLayout : MachineInventoryLayout {
        public List<AdvancedInventoryLayout> inputInventories;
        public List<AdvancedInventoryLayout> outputInventories;
    }
}

