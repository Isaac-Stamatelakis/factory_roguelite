using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Newtonsoft.Json;
using RecipeModule.Transmutation;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using RecipeModule.Processors;
using RecipeModule;
using Items.Inventory;

namespace TileEntityModule.Instances.Machines
{
    public class ProcessingMachineInstance : TileEntityInstance<ProcessingMachine>, ITickableTileEntity, IRightClickableTileEntity, ISerializableTileEntity, IConduitInteractable, ISolidItemConduitInteractable, IFluidConduitInteractable, IEnergyConduitInteractable, ISignalConduitInteractable, IProcessorTileEntity, IInventoryListener
    {
        private StandardMachineInventory inventory;
        private IMachineRecipe currentRecipe;
        private int currentRecipeEnergy;
        private int currentRecipeCost;

        public ProcessingMachineInstance(ProcessingMachine tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
            if (inventory == null) {
                inventory = StandardMachineInventoryFactory.initalize((StandardMachineInventoryLayout) tileEntity.Processor.getInventoryLayout());
            }
        }

        public void onRightClick()   
        {
            tileEntity.Processor.displayTileEntity(inventory,tileEntity.Tier,tileEntity.name,this);
        }
        

        public string serialize()
        {
            return StandardMachineInventoryFactory.serialize(inventory);
        }

        public void tickUpdate()
        {
            if (currentRecipe == null) {
                return;
            }
            processRecipe();

        }

        private void initRecipe() {
            

        }

        private void processRecipe() {
            if (inventory.Energy <= 0) {
                return;
            }
            int energyToUse = Mathf.Min(inventory.Energy, currentRecipeCost);
            inventory.Energy -= energyToUse;
            currentRecipeCost -= energyToUse;
            
            if (currentRecipeCost > 0) {
                return;
            }
            if (currentRecipeCost < 0) {
                inventory.Energy-=currentRecipeCost;
            }   
            List<ItemSlot> recipeOut = currentRecipe.getOutputs();
            List<ItemSlot> solidOutputs;
            List<ItemSlot> fluidOutputs;
            ItemSlotHelper.sortInventoryByState(recipeOut, out solidOutputs, out fluidOutputs);
            ItemSlotHelper.insertListIntoInventory(inventory.ItemOutputs.Slots,solidOutputs,Global.MaxSize);
            ItemSlotHelper.insertListIntoInventory(inventory.FluidOutputs.Slots,fluidOutputs,tileEntity.Tier.getFluidStorage());
            currentRecipe = null;
            inventoryUpdate(0);
        }

        public void inventoryUpdate(int n) {
            if (currentRecipe != null) {
                return;
            }
            currentRecipe = tileEntity.Processor.getRecipe(
                mode: inventory.Mode,
                solidInputs: inventory.ItemInputs.Slots,
                solidOutputs: inventory.ItemOutputs.Slots,
                fluidInputs: inventory.FluidInputs.Slots,
                fluidOutputs: inventory.FluidOutputs.Slots
            );
            if (currentRecipe == null) {
                return;
            }
            currentRecipeEnergy = currentRecipe.getTotalEnergyCost();
            currentRecipeCost = currentRecipe.getEnergyCostPerTick();
        }


        public void unserialize(string data)
        {
            inventory = StandardMachineInventoryFactory.deserialize(data);
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return tileEntity.ConduitLayout;
        }

        public ItemSlot extractItem(Vector2Int portPosition)
        {
            foreach (ItemSlot itemSlot in inventory.ItemOutputs.Slots) {
                if (itemSlot != null && itemSlot.itemObject != null) {
                    return itemSlot;
                }
            }
            return null;
        }

        public void insertItem(ItemSlot itemSlot,Vector2Int portPosition)
        {
            inventoryUpdate(0);
            if (itemSlot == null || itemSlot.itemObject == null) {
                return;
            }
            ItemSlotHelper.insertIntoInventory(inventory.ItemInputs.Slots,itemSlot,Global.MaxSize);
        }

        public ItemSlot extractFluid(Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        public bool insertFluid(ItemSlot itemSlot,Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        public int insertEnergy(int insertEnergy,Vector2Int portPosition)
        {
            if (inventory.Energy >= tileEntity.Tier.getEnergyStorage()) {
                return 0;
            }
            int sum = inventory.Energy+=insertEnergy;
            if (sum > tileEntity.Tier.getEnergyStorage()) {
                inventory.Energy=tileEntity.Tier.getEnergyStorage();
                Debug.Log(sum-tileEntity.Tier.getEnergyStorage());
                return sum - tileEntity.Tier.getEnergyStorage();
            }
            inventory.Energy = sum;
            return insertEnergy;
        }

        public void insertSignal(int signal,Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        public int extractSignal(Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        public ref int getEnergy(Vector2Int portPosition)
        {
            return ref inventory.energy;
        }

        public RecipeProcessor getRecipeProcessor()
        {
            return tileEntity.Processor;
        }

        public ItemSlot extractSolidItem(Vector2Int portPosition)
        {
            return ItemSlotHelper.extractFromInventory(inventory.ItemOutputs.Slots);
        }

        public void insertSolidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            ItemSlotHelper.insertIntoInventory(inventory.ItemInputs.Slots, itemSlot, Global.MaxSize);
        }

        public ItemSlot extractFluidItem(Vector2Int portPosition)
        {
            return ItemSlotHelper.extractFromInventory(inventory.FluidOutputs.Slots);
        }

        public void insertFluidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            ItemSlotHelper.insertIntoInventory(inventory.FluidInputs.Slots, itemSlot, tileEntity.Tier.getFluidStorage());
        }
    }
}

