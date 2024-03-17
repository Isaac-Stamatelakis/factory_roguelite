using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule.Inventory;

namespace TileEntityModule.Instances.Machines {
    [CreateAssetMenu(fileName = "New Machine Layout", menuName = "Tile Entity/Machine/Layout/Standard")]
    public class StandardMachineInventoryLayout : TypedInventoryLayout<StandardSolidAndFluidInventory> {
        [SerializeField] public List<Vector2Int> itemInputs;
        [SerializeField] public List<Vector2Int> itemOutputs;
        [SerializeField] public List<Vector2Int> fluidInputs;
        [SerializeField] public List<Vector2Int> fluidOutputs;

        public override void display(Transform parent, StandardSolidAndFluidInventory inventory, InventoryUIMode uIType)
        {
            MachineUIFactory.initInventory(inventory.ItemInputs.Slots,itemInputs,ItemState.Solid,"SolidInputs",parent,uIType);
            MachineUIFactory.initInventory(inventory.ItemOutputs.Slots,itemOutputs,ItemState.Solid,"SolidOutputs",parent,uIType);
            MachineUIFactory.initInventory(inventory.FluidInputs.Slots,fluidInputs,ItemState.Fluid,"FluidInputs",parent,uIType);
            MachineUIFactory.initInventory(inventory.FluidOutputs.Slots,fluidOutputs,ItemState.Fluid,"FluidOutputs",parent,uIType);
        }
    }
}