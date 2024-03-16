using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule.Inventory;

namespace TileEntityModule.Instances.Machines {
    public abstract class MachineInventory<Layout> : IMachineInventory where Layout : InventoryLayout
    {
        public abstract void display(InventoryLayout layout, Transform parent);
    }


}

