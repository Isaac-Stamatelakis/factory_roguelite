using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items.Inventory;
using Items;

namespace TileEntityModule.Instances.Machines {
    [CreateAssetMenu(fileName = "New Machine Layout", menuName = "Tile Entity/Machine/Layout/Standard")]
    public class StandardMachineInventoryLayout : TypedInventoryLayout<StandardSolidAndFluidInventory> {
        [SerializeField] public List<Vector2Int> itemInputs;
        [SerializeField] public List<Vector2Int> itemOutputs;
        [SerializeField] public List<Vector2Int> fluidInputs;
        [SerializeField] public List<Vector2Int> fluidOutputs;

        public override void display(Transform parent, StandardSolidAndFluidInventory inventory, InventoryUIMode uIType,IInventoryListener listener)
        {
            MachineUIFactory.initInventory(inventory.ItemInputs.Slots,itemInputs,ItemState.Solid,"SolidInputs",parent,uIType,listener);
            MachineUIFactory.initInventory(inventory.ItemOutputs.Slots,itemOutputs,ItemState.Solid,"SolidOutputs",parent,uIType,listener);
            MachineUIFactory.initInventory(inventory.FluidInputs.Slots,fluidInputs,ItemState.Fluid,"FluidInputs",parent,uIType,listener);
            MachineUIFactory.initInventory(inventory.FluidOutputs.Slots,fluidOutputs,ItemState.Fluid,"FluidOutputs",parent,uIType,listener);
        }
    }
}