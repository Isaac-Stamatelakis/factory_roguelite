using System;
using System.Collections.Generic;
using Item.Inventory;
using Item.Slot;
using Items;
using Items.Inventory;
using JetBrains.Annotations;
using Recipe.Data;
using Recipe.Objects;
using Recipe.Processor;
using Recipe.Viewer;
using RecipeModule;
using Unity.VisualScripting;
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

            if (recipe is TransmutationDisplayableRecipe transmutationDisplayableRecipe)
            {
                var inputs = new List<List<ItemSlot>>();
                foreach (ItemSlot inputSlot in transmutationDisplayableRecipe.Inputs)
                {
                    inputs.Add(new List<ItemSlot>{ inputSlot});
                }
                var outputs = new List<List<ItemSlot>>();
                foreach (ItemSlot outputSlot in transmutationDisplayableRecipe.Outputs)
                {
                    outputs.Add(new List<ItemSlot>{outputSlot});
                }
                InitializeTransmutationSwitchUIRecipe(
                    solidInputUI, 
                    transmutationDisplayableRecipe.InputState == ItemState.Solid ? inputs : null, 
                    layoutObject.SolidInputs
                );
                InitializeTransmutationSwitchUIRecipe(
                    fluidInputUI, 
                    transmutationDisplayableRecipe.InputState == ItemState.Fluid ? inputs : null, 
                    layoutObject.FluidInputs
                );
                InitializeTransmutationSwitchUIRecipe(
                    solidOutputUI, 
                    transmutationDisplayableRecipe.OutputState == ItemState.Solid ? outputs : null, 
                    layoutObject.SolidOutputs
                );
                InitializeTransmutationSwitchUIRecipe(
                    fluidOutputUI, 
                    transmutationDisplayableRecipe.OutputState == ItemState.Solid ? outputs : null, 
                    layoutObject.FluidOutputs
                );
                
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

        private void InitializeTransmutationSwitchUIRecipe(InventoryUI inventoryUI, List<List<ItemSlot>> inventories, MachineInventoryOptions inventoryOptions)
        {
            int size = inventoryOptions.GetIntSize();
            if (size == 0)
            {
                inventoryUI.gameObject.SetActive(false);
                return;
            }
            InventoryUIRotator rotator = inventoryUI.GetComponent<InventoryUIRotator>();
            if (ReferenceEquals(rotator, null))
            {
                rotator = inventoryUI.AddComponent<InventoryUIRotator>();
            }
            rotator.Initialize(inventories,size,100);
            inventoryUI.SetInteractMode(InventoryInteractMode.Recipe);
        }
    }
}
