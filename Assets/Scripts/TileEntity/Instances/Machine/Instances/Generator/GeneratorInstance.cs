using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Newtonsoft.Json;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using Items.Inventory;
using Recipe;
using Recipe.Data;
using Recipe.Processor;
using RecipeModule;
using TileEntity.Instances.WorkBenchs;
using UI;

namespace TileEntity.Instances.Machines
{
    
    public class GeneratorInstance : TileEntityInstance<Generator>, ITickableTileEntity, 
        IRightClickableTileEntity, ISerializableTileEntity, IProcessorTileEntity, IInventoryListener, IConduitTileEntityAggregator
    {
        public GeneratorInstance(Generator tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
            /*
            if (inventory == null) {
                inventory = StandardMachineInventoryFactory.initalize(this.tileEntity.StandardMachineLayout);
            }
            */
        }

        private int mode;
        private MachineItemInventory itemInventory;
        private MachineEnergyInventory energyInventory;
        private GeneratorItemRecipe currentRecipe;
        public void onRightClick()
        {
            TileEntityHelper.DisplayTileEntityUI<GeneratorInstance>(TileEntityObject.RecipeProcessor.UIPrefab, this);
        }
        

        public string serialize()
        {
            SerializedGeneratorData serializedGeneratorData = new SerializedGeneratorData(
                mode,
                MachineInventoryFactory.SerializeItemMachineInventory(itemInventory),
                MachineInventoryFactory.SerializedEnergyMachineInventory(energyInventory),
                RecipeSerializationFactory.Serialize(currentRecipe, RecipeType.Generator)
            );
            return JsonConvert.SerializeObject(serializedGeneratorData);
        }

        public void tickUpdate()
        {
            InventoryUpdate(0); // ONLY HERE FOR TESTING PURPOSES VERY INEFFICENT
            if (currentRecipe == null) {
                return;
            }
            processRecipe();

        }

        private void processRecipe() {
            /*
            if (remainingTicks > 0) {
                if (inventory.Energy < tileEntity.Tier.GetEnergyStorage()) {
                    remainingTicks--;
                    inventory.Energy += currentRecipe.getEnergyPerTick();
                }
            }
            if (remainingTicks <= 0) {
                currentRecipe = null;
            }
            */
        }

        public void InventoryUpdate(int n) {
            if (currentRecipe != null) {
                return;
            }
            currentRecipe = RecipeRegistry.GetProcessorInstance(TileEntityObject.RecipeProcessor).GetRecipe<GeneratorItemRecipe>(
                mode, 
                itemInventory.itemInputs,
                itemInventory.fluidInputs
            );
        }
        /*
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
        */

        public void unserialize(string data)
        {
            //inventory = StandardMachineInventoryFactory.deserialize(data);
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.Layout;
        }

        public IConduitInteractable GetConduitInteractable(ConduitType conduitType)
        {
            return conduitType switch
            {
                ConduitType.Item or ConduitType.Fluid => itemInventory,
                ConduitType.Energy => energyInventory,
                _ => null
            };
        }

        public RecipeProcessor GetRecipeProcessor()
        {
            return TileEntityObject.RecipeProcessor;
        }

        private class SerializedGeneratorData
        {
            public int Mode;
            public string SerializedMachineInventory;
            public string SerializedEnergyInventory;
            public string SerializedGeneratorRecipe;

            public SerializedGeneratorData(int mode, string serializedMachineInventory, string serializedEnergyInventory, string serializedGeneratorRecipe)
            {
                Mode = mode;
                SerializedMachineInventory = serializedMachineInventory;
                SerializedEnergyInventory = serializedEnergyInventory;
                SerializedGeneratorRecipe = serializedGeneratorRecipe;
            }
        }
    }
}

