using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Chunks;
using Newtonsoft.Json;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using RecipeModule;
using Items.Inventory;
using Recipe;
using Recipe.Data;
using Recipe.Processor;
using TileEntity.Instances.Machine.Instances;
using UI;

namespace TileEntity.Instances.Machines
{
    public class ProcessingMachineInstance : MachineInstance<ProcessingMachine, ItemEnergyRecipe>
    {
        public ProcessingMachineInstance(ProcessingMachine tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
            Debug.Log("Initalized");
            InitializeItemInventory();
            InitializeEnergyInventory();
        }

        public override string serialize()
        {
            try
            {
                SerializedProcessingMachine serializedProcessingMachine = new SerializedProcessingMachine(
                    Mode,
                    MachineInventoryFactory.SerializeItemMachineInventory(Inventory),
                    MachineInventoryFactory.SerializedEnergyMachineInventory(EnergyInventory),
                    RecipeSerializationFactory.Serialize(currentRecipe, RecipeType.EnergyItem)
                );
                return JsonConvert.SerializeObject(serializedProcessingMachine);
            }
            catch (NullReferenceException e)
            {
                Debug.LogWarning(e);
                return null;
            }
        }
        
        public override void unserialize(string data)
        {
            Debug.Log("Unserialized");
            //inventory = StandardMachineInventoryFactory.deserialize(data);
        }

        public override void tickUpdate()
        {
            if (currentRecipe == null) {
                return;
            }
            ProcessRecipe();
        }

        private void ProcessRecipe() {
            /*
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
            List<ItemSlot> recipeOut = currentRecipe.Outputs;
            ItemSlotHelper.sortInventoryByState(recipeOut, out var solidOutputs, out var fluidOutputs);
            ItemSlotHelper.InsertListIntoInventory(inventory.ItemOutputs.Slots,solidOutputs,Global.MaxSize);
            ItemSlotHelper.InsertListIntoInventory(inventory.FluidOutputs.Slots,fluidOutputs,tileEntity.Tier.GetFluidStorage());
            currentRecipe = null;
            inventoryUpdate(0);
            */
        }

        public override void InventoryUpdate(int n) {
            /*
            if (currentRecipe != null) {
                return;
            }
            currentRecipe = RecipeRegistry.GetInstance().LookUpRecipe(inventory.ItemInputs.Slots, inventory.FluidInputs.Slots);
            */
            
        }

        public override float GetProgressPercent()
        {
            return 0;
        }

        private class SerializedProcessingMachine
        {
            public int Mode;
            public string SerializedMachineInventory;
            public string SerializedEnergyInventory;
            public string SerializedGeneratorRecipe;

            public SerializedProcessingMachine(int mode, string serializedMachineInventory, string serializedEnergyInventory, string serializedGeneratorRecipe)
            {
                Mode = mode;
                SerializedMachineInventory = serializedMachineInventory;
                SerializedEnergyInventory = serializedEnergyInventory;
                SerializedGeneratorRecipe = serializedGeneratorRecipe;
            }
        }
    }
}

