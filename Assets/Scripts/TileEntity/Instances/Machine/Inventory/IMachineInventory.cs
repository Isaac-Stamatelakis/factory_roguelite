using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntityModule.Instances.Machines {
    public interface IMachineInventory {
        public void display(MachineInventoryLayout layout,Transform parent);
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