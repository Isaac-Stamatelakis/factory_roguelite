using System.Collections.Generic;
using Item.Slot;
using Items.Inventory;
using UnityEngine;

namespace TileEntity.Instances.Machine.UI
{
    public class MachineInventoryUI : MonoBehaviour
    {
        [SerializeField] private InventoryUI solidInputUI;
        [SerializeField] private InventoryUI solidOutputUI;
        [SerializeField] private InventoryUI fluidInputUI;
        [SerializeField] private InventoryUI fluidOutputUI;
        private MachineItemInventory displayedInventory;
        public void Display(MachineItemInventory machineItemInventory)
        {
            MachineLayoutObject layoutObject = machineItemInventory.Parent.GetMachineLayout();
            bool error = false;
            if (ReferenceEquals(layoutObject,null))
            {
                Debug.LogWarning($"'MachineInventoryUI' Tried to display inventory with no layout for {machineItemInventory.Parent.getName()}");
                error = true;
            }
            if (error) return;
            
            displayedInventory = machineItemInventory;
            InitializeInventoryUI(solidInputUI, machineItemInventory.itemInputs,layoutObject.SolidInputs);
            InitializeInventoryUI(solidOutputUI, machineItemInventory.itemOutputs,layoutObject.SolidOutputs);
            InitializeInventoryUI(fluidInputUI, machineItemInventory.fluidInputs,layoutObject.FluidInputs);
            InitializeInventoryUI(fluidOutputUI, machineItemInventory.fluidOutputs,layoutObject.FluidOutputs);
            
        }
        public void FixedUpdate()
        {
            solidInputUI.RefreshSlots();
            solidOutputUI.RefreshSlots();
            fluidInputUI.RefreshSlots();
            fluidOutputUI.RefreshSlots();
        }
        
        private void InitializeInventoryUI(InventoryUI inventoryUI, List<ItemSlot> inventory, MachineInventoryOptions inventoryOptions)
        {
            int size = inventoryOptions.GetIntSize();
            if (size == 0)
            {
                inventoryUI.gameObject.SetActive(false);
                return;
            }

            if (inventory == null)
            {
                Debug.LogWarning($"'MachineBaseUI' Tried to display '{inventoryUI.name}' for {displayedInventory.Parent.getName()} which was null");
                return;
            }
            if (!inventoryOptions.DefaultOffset)
            {
                inventoryUI.transform.position = (Vector2)inventoryOptions.Offset;
            }
            inventoryUI.DisplayInventory(inventory);
            inventoryUI.AddListener(displayedInventory.Parent);
        }
    }
}
