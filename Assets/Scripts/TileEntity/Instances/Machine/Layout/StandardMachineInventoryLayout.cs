using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items.Inventory;

namespace TileEntityModule.Instances.Machines {
    [CreateAssetMenu(fileName = "New Machine Layout", menuName = "Tile Entity/Machine/Layout/Standard")]
    public class StandardMachineInventoryLayout : TypedInventoryLayout<StandardSolidAndFluidInventory> {
        [SerializeField] public List<Vector2Int> itemInputs;
        [SerializeField] public List<Vector2Int> itemOutputs;
        [SerializeField] public List<Vector2Int> fluidInputs;
        [SerializeField] public List<Vector2Int> fluidOutputs;

        public override void display(Transform parent, StandardSolidAndFluidInventory inventory, InventoryUIMode uIType,IInventoryListener listener)
        {
            GameObject solidSlotPrefab = AddressableLoader.getPrefabInstantly("Assets/UI/Inventory/ItemInventorySlot.prefab");
            GameObject fluidSlotPrefab = AddressableLoader.getPrefabInstantly("Assets/UI/Inventory/FluidInventorySlot.prefab");
            MachineUIFactory.initInventory(inventory.ItemInputs.Slots,itemInputs,ItemState.Solid,"SolidInputs",parent,uIType,listener,solidSlotPrefab);
            MachineUIFactory.initInventory(inventory.ItemOutputs.Slots,itemOutputs,ItemState.Solid,"SolidOutputs",parent,uIType,listener,solidSlotPrefab);
            MachineUIFactory.initInventory(inventory.FluidInputs.Slots,fluidInputs,ItemState.Fluid,"FluidInputs",parent,uIType,listener,fluidSlotPrefab);
            MachineUIFactory.initInventory(inventory.FluidOutputs.Slots,fluidOutputs,ItemState.Fluid,"FluidOutputs",parent,uIType,listener,fluidSlotPrefab);
        }
    }
}