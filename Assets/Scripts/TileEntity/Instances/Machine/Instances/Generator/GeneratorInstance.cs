using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Newtonsoft.Json;
using RecipeModule.Transmutation;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using Items.Inventory;
using RecipeModule;

namespace TileEntityModule.Instances.Machines
{
    
    public class GeneratorInstance : TileEntityInstance<Generator>, ITickableTileEntity, IRightClickableTileEntity, ISerializableTileEntity, IConduitInteractable, ISolidItemConduitInteractable, IFluidConduitInteractable, IEnergyConduitInteractable, ISignalConduitInteractable, IProcessorTileEntity, IInventoryListener
    {
        public GeneratorInstance(Generator tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
            if (inventory == null) {
                inventory = StandardMachineInventoryFactory.initalize((StandardMachineInventoryLayout)tileEntity.EnergyRecipeProcessor.getInventoryLayout());
            }
        }
        private StandardMachineInventory inventory;
        private IEnergyProduceRecipe currentRecipe;
        private int remainingTicks;
        public void onRightClick()   
        {
            tileEntity.EnergyRecipeProcessor.displayTileEntity(inventory,tileEntity.Tier,tileEntity.name,this);
        }
        

        public string serialize()
        {
            return StandardMachineInventoryFactory.serialize(inventory);
        }

        public void tickUpdate()
        {
            inventoryUpdate(0); // ONLY HERE FOR TESTING PURPOSES VERY INEFFICENT
            if (currentRecipe == null) {
                return;
            }
            processRecipe();

        }

        private void processRecipe() {
            if (remainingTicks > 0) {
                if (inventory.Energy < tileEntity.Tier.getEnergyStorage()) {
                    remainingTicks--;
                    inventory.Energy += currentRecipe.getEnergyPerTick();
                }
                
            }
            if (remainingTicks <= 0) {
                currentRecipe = null;
            }
        }

        public void inventoryUpdate(int n) {
            if (currentRecipe != null) {
                return;
            }
            currentRecipe = processEnergyRecipes();
            if (currentRecipe == null) {
                return;
            }
            remainingTicks = currentRecipe.getLifespan();
            
        }

        private IEnergyProduceRecipe processEnergyRecipes() {
            if (tileEntity.EnergyRecipeProcessor == null) {
                return null;
            }
            IEnergyProduceRecipe recipe = tileEntity.EnergyRecipeProcessor.getEnergyRecipe(
                mode: inventory.Mode,
                solidInputs: inventory.ItemInputs.Slots,
                solidOutputs: inventory.ItemOutputs.Slots,
                fluidInputs: inventory.FluidInputs.Slots,
                fluidOutputs: inventory.FluidOutputs.Slots
            );
            if (recipe == null) {
                return null;
            }
            if (recipe is not IEnergyProduceRecipe energyProduceRecipe) {
                Debug.LogError("Machine '" + tileEntity.name + "' Reciped assigned to machine");
                return null;
            }
            return recipe;
            
        }

        public void unserialize(string data)
        {
            inventory = StandardMachineInventoryFactory.deserialize(data);
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return tileEntity.Layout;
        }


        public void insertSolidItem(ItemSlot itemSlot,Vector2Int portPosition)
        {
            ItemSlotHelper.insertIntoInventory(inventory.ItemInputs.Slots,itemSlot,Global.MaxSize);
        }

        public int insertEnergy(int insertEnergy,Vector2Int portPosition)
        {
            inventory.Energy += insertEnergy;
            return 0;
        }

        public void insertSignal(bool signal,Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        public bool extractSignal(Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

    

        public ref int getEnergy(Vector2Int portPosition)
        {
            return ref inventory.energy;
        }

        

        public RecipeProcessor getRecipeProcessor()
        {
            return tileEntity.EnergyRecipeProcessor;
        }

        public ItemSlot extractSolidItem(Vector2Int portPosition)
        {
            return ItemSlotHelper.extractFromInventory(inventory.ItemOutputs.Slots);
        }

        public ItemSlot extractFluidItem(Vector2Int portPosition)
        {
            return ItemSlotHelper.extractFromInventory(inventory.FluidOutputs.Slots);
            
        }

        public void insertFluidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            ItemSlotHelper.insertIntoInventory(inventory.FluidInputs.Slots,itemSlot,tileEntity.Tier.getFluidStorage());
        }
    }
}

