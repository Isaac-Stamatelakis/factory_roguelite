using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntityModule.Instances.Machines {
    public abstract class MachineInventory<Layout> : IMachineInventory where Layout : MachineInventoryLayout
    {
        public abstract void display(MachineInventoryLayout layout, Transform parent);
    }


}

