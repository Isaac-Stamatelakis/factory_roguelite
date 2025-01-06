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
    public class TileEntityInventoryUI : MonoBehaviour
    {
        [SerializeField] protected InventoryUI solidInputUI;
        [SerializeField] protected InventoryUI solidOutputUI;
        [SerializeField] protected InventoryUI fluidInputUI;
        [SerializeField] protected InventoryUI fluidOutputUI;
        private ITileEntityInstance displayedTileEntity;
        
        public void Display(TileEntityInventory machineItemInventory, TileEntityLayoutObject layoutObject, ITileEntityInstance tileEntityInstance)
        {
            this.displayedTileEntity = tileEntityInstance;
            InitializeInventoryUI(solidInputUI, machineItemInventory.itemInputs,layoutObject?.SolidInputs);
            InitializeInventoryUI(solidOutputUI, machineItemInventory.itemOutputs,layoutObject?.SolidOutputs);
            InitializeInventoryUI(fluidInputUI, machineItemInventory.fluidInputs,layoutObject?.FluidInputs);
            InitializeInventoryUI(fluidOutputUI, machineItemInventory.fluidOutputs,layoutObject?.FluidOutputs);
        }

        public void DisplayRecipe(DisplayableRecipe recipe)
        {
            TileEntityLayoutObject layoutObject = recipe.RecipeData.ProcessorInstance.RecipeProcessorObject.LayoutObject;
            if (recipe is ItemDisplayableRecipe itemDisplayableRecipe)
            {
                InitializeInventoryUIRecipe(solidInputUI, itemDisplayableRecipe.SolidInputs, layoutObject?.SolidInputs);
                InitializeInventoryUIRecipe(solidOutputUI, itemDisplayableRecipe.SolidOutputs, layoutObject?.SolidOutputs);
                InitializeInventoryUIRecipe(fluidInputUI, itemDisplayableRecipe.FluidInputs, layoutObject?.FluidInputs);
                InitializeInventoryUIRecipe(fluidOutputUI, itemDisplayableRecipe.FluidOutputs, layoutObject?.FluidOutputs);
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
                    layoutObject?.SolidInputs
                );
                InitializeTransmutationSwitchUIRecipe(
                    fluidInputUI, 
                    transmutationDisplayableRecipe.InputState == ItemState.Fluid ? inputs : null, 
                    layoutObject?.FluidInputs
                );
                InitializeTransmutationSwitchUIRecipe(
                    solidOutputUI, 
                    transmutationDisplayableRecipe.OutputState == ItemState.Solid ? outputs : null, 
                    layoutObject?.SolidOutputs
                );
                InitializeTransmutationSwitchUIRecipe(
                    fluidOutputUI, 
                    transmutationDisplayableRecipe.OutputState == ItemState.Solid ? outputs : null, 
                    layoutObject?.FluidOutputs
                );
                
            }
        }
        public void FixedUpdate()
        {
            solidInputUI?.RefreshSlots();
            solidOutputUI?.RefreshSlots();
            fluidInputUI?.RefreshSlots();
            fluidOutputUI?.RefreshSlots();
        }
        
        private void InitializeInventoryUI(InventoryUI inventoryUI, List<ItemSlot> inventory, MachineInventoryOptions inventoryOptions)
        {
            if (ReferenceEquals(inventoryUI, null)) return;
            int size = GetSize(inventoryOptions, inventory);
            if (size == 0)
            {
                inventoryUI.gameObject.SetActive(false);
                return;
            }

            if (inventory == null)
            {
                Debug.LogWarning($"'MachineBaseUI' Tried to display '{inventoryUI.name}' for {displayedTileEntity.getName()} which was null");
                return;
            }
            inventoryUI.DisplayInventory(inventory);
            if (displayedTileEntity is IInventoryListener inventoryListener)
            {
                inventoryUI.AddListener(inventoryListener);
            }
            
        }

        private int GetSize(MachineInventoryOptions options, List<ItemSlot> inventory)
        {
            if (ReferenceEquals(options,null)) return inventory.Count;
            return options.GetIntSize();
        }

        private void InitializeInventoryUIRecipe(InventoryUI inventoryUI, List<ItemSlot> items, MachineInventoryOptions inventoryOptions)
        {
            if (ReferenceEquals(inventoryUI, null)) return;
            int size = GetSize(inventoryOptions, items);
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
            if (ReferenceEquals(inventoryUI, null)) return;
            int size = inventories == null || inventories.Count == 0 ? 0 : GetSize(inventoryOptions, inventories[0]);
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
