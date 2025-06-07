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
    public class TileEntityInventoryUI : MonoBehaviour, IInventoryUITileEntityUI
    {
        public const int TRANSMUTATION_ROTATE_RATE = 50;
        [SerializeField] public InventoryUI solidInputUI;
        [SerializeField] public InventoryUI solidOutputUI;
        [SerializeField] public InventoryUI fluidInputUI;
        [SerializeField] public InventoryUI fluidOutputUI;
        private ITileEntityInstance displayedTileEntity;
        public void Display(TileEntityInventory machineItemInventory, TileEntityLayoutObject layoutObject, ITileEntityInstance tileEntityInstance)
        {
            this.displayedTileEntity = tileEntityInstance;
            InitializeInventoryUI(solidInputUI, machineItemInventory.itemInputs,layoutObject?.SolidInputs);
            InitializeInventoryUI(solidOutputUI, machineItemInventory.itemOutputs,layoutObject?.SolidOutputs);
            InitializeInventoryUI(fluidInputUI, machineItemInventory.fluidInputs,layoutObject?.FluidInputs);
            
            InitializeInventoryUI(fluidOutputUI, machineItemInventory.fluidOutputs,layoutObject?.FluidOutputs);
            if (solidOutputUI) solidOutputUI.SetInteractMode(InventoryInteractMode.BlockInput);
            if (fluidOutputUI) fluidOutputUI.SetInteractMode(InventoryInteractMode.BlockInput);
            
            TileItem tileItem = displayedTileEntity.GetTileItem();
            Tier tier = tileItem.GetTier();
            uint fluidStorage = tier.GetFluidStorage();
            fluidInputUI.SetMaxSize(fluidStorage);
            fluidOutputUI.SetMaxSize(fluidStorage);
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

                int randomIndex = transmutationDisplayableRecipe.InitialDisplayIndex;
                InitializeTransmutationSwitchUIRecipe(
                    solidInputUI, 
                    transmutationDisplayableRecipe.InputState == ItemState.Solid ? inputs : null, 
                    layoutObject?.SolidInputs,
                    randomIndex
                );
                InitializeTransmutationSwitchUIRecipe(
                    fluidInputUI, 
                    transmutationDisplayableRecipe.InputState == ItemState.Fluid ? inputs : null, 
                    layoutObject?.FluidInputs,
                    randomIndex
                );
                InitializeTransmutationSwitchUIRecipe(
                    solidOutputUI, 
                    transmutationDisplayableRecipe.OutputState == ItemState.Solid ? outputs : null, 
                    layoutObject?.SolidOutputs,
                    randomIndex
                );
                InitializeTransmutationSwitchUIRecipe(
                    fluidOutputUI, 
                    transmutationDisplayableRecipe.OutputState == ItemState.Solid ? outputs : null, 
                    layoutObject?.FluidOutputs,
                    randomIndex
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

        private int SizeCheckInventoryUI<T>(InventoryUI inventoryUI, List<T> inventory, MachineInventoryOptions inventoryOptions) where T : ItemSlot
        {
            if (ReferenceEquals(inventoryUI, null)) return 0;
            int size = GetSize(inventoryOptions, inventory);
            if (size != 0) return size;
            GlobalHelper.DeleteAllChildren(inventoryUI.transform);
            return 0;
        }
        
        private void InitializeInventoryUI(InventoryUI inventoryUI, List<ItemSlot> inventory, MachineInventoryOptions inventoryOptions)
        {
            int size = SizeCheckInventoryUI(inventoryUI,inventory,inventoryOptions);
            if (size == 0) return;
            
            inventoryUI.DisplayInventory(inventory);
            if (displayedTileEntity is IMachineInstance machineInstance)
            {
                inventoryUI.AddCallback(machineInstance.InventoryUpdate);
            }
            
        }

        private int GetSize<T>(MachineInventoryOptions options, List<T> inventory) where T : ItemSlot
        {
            if (ReferenceEquals(options,null)) return inventory.Count;
            return options.GetIntSize();
        }

        private void InitializeInventoryUIRecipe<T>(InventoryUI inventoryUI, List<T> items, MachineInventoryOptions inventoryOptions) where T : ItemSlot
        {
            int size = SizeCheckInventoryUI(inventoryUI,items,inventoryOptions);
            if (size == 0) return;
            
            InventoryUIRotator rotator = inventoryUI.GetComponent<InventoryUIRotator>();
            bool rotatorExists = rotator;
            if (rotatorExists)
            {
                GameObject.Destroy(rotator);
            }
            
            List<string> topNames = new List<string>();
            foreach (T item in items)
            {
                if (item is ChanceItemSlot chanceItemSlot)
                {
                    float chance = chanceItemSlot.chance;
                    topNames.Add(chance < 1 ? $"{chance:P0}".Replace(" ",string.Empty) : string.Empty);
                }
            }

            List<ItemSlot> castItems = items.ConvertAll(item => (ItemSlot)item);
            inventoryUI.DisplayInventory(castItems,size);
            
            if (topNames.Count > 0) inventoryUI.DisplayTopText(topNames);
            inventoryUI.SetInteractMode(InventoryInteractMode.Recipe);
        }

        private void InitializeTransmutationSwitchUIRecipe(InventoryUI inventoryUI, List<List<ItemSlot>> inventories, MachineInventoryOptions inventoryOptions, int initialIndex)
        {
            if (inventories == null || inventories.Count == 0)
            {
                if (!ReferenceEquals(inventoryUI,null)) GlobalHelper.DeleteAllChildren(inventoryUI.transform);
                return;
            }
            int size = SizeCheckInventoryUI(inventoryUI,inventories[0],inventoryOptions);
            if (size == 0) return;
            
            InventoryUIRotator rotator = inventoryUI.GetComponent<InventoryUIRotator>();
            if (ReferenceEquals(rotator, null))
            {
                rotator = inventoryUI.AddComponent<InventoryUIRotator>();
            }
            rotator.Initialize(inventories,size,TRANSMUTATION_ROTATE_RATE,initialIndex:initialIndex);
            inventoryUI.SetInteractMode(InventoryInteractMode.Recipe);
        }

        public InventoryUI GetInput()
        {
            return solidInputUI;
        }

        public List<InventoryUI> GetAllInventoryUIs()
        {
            return new List<InventoryUI>
            {
                solidInputUI,
                solidOutputUI,
                fluidInputUI,
                fluidOutputUI,
            };
        }
    }
}
