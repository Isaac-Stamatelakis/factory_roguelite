using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items.Inventory;

namespace TileEntityModule.Instances.Machines {
    public interface IMachineInventory {
        public void display(InventoryLayout layout,Transform parent);
    }

    public interface IEnergyMachineInventory {

    }

    public interface IInputMachineInventory {
        
    }

    public interface IOutputMachineInventory {

    }

    public interface IBatterySlotInventory {

    }

}