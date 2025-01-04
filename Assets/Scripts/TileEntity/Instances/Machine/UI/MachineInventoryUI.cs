using System.Collections.Generic;
using Item.Slot;
using Items;
using Items.Inventory;
using JetBrains.Annotations;
using Recipe.Data;
using Recipe.Objects;
using Recipe.Processor;
using Recipe.Viewer;
using RecipeModule;
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

        public void DisplayRecipe(DisplayableRecipe recipe)
        {
            if (recipe.RecipeData.ProcessorInstance.RecipeProcessorObject is not MachineRecipeProcessor machineRecipeProcessor)
            {
                Debug.LogWarning("Displaying machine inventory ui with non machine recipe processor");
                return;
            }
            MachineLayoutObject layoutObject = machineRecipeProcessor.MachineLayout;
            if (recipe is ItemDisplayableRecipe itemDisplayableRecipe)
            {
                InitializeInventoryUIRecipe(solidInputUI, itemDisplayableRecipe.SolidInputs, layoutObject.SolidInputs);
                InitializeInventoryUIRecipe(solidOutputUI, itemDisplayableRecipe.SolidOutputs, layoutObject.SolidOutputs);
                InitializeInventoryUIRecipe(fluidInputUI, itemDisplayableRecipe.FluidInputs, layoutObject.FluidInputs);
                InitializeInventoryUIRecipe(fluidOutputUI, itemDisplayableRecipe.FluidOutputs, layoutObject.FluidOutputs);
            }
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

        private void InitializeInventoryUIRecipe(InventoryUI inventoryUI, List<ItemSlot> items, MachineInventoryOptions inventoryOptions)
        {
            int size = inventoryOptions.GetIntSize();
            if (size == 0)
            {
                inventoryUI.gameObject.SetActive(false);
                return;
            }
            inventoryUI.DisplayInventory(items,size);
            inventoryUI.SetInteractMode(InventoryInteractMode.Recipe);
        }
    }
}
