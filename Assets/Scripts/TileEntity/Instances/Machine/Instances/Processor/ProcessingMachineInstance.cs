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
        }

        public override string serialize()
        {
            SerializedProcessingMachine serializedProcessingMachine = new SerializedProcessingMachine(
                Mode,
                MachineInventoryFactory.SerializeItemMachineInventory(Inventory),
                MachineInventoryFactory.SerializedEnergyMachineInventory(EnergyInventory),
                RecipeSerializationFactory.Serialize(currentRecipe, RecipeType.Machine)
            );
            return JsonConvert.SerializeObject(serializedProcessingMachine);
        }
        
        public override void unserialize(string data)
        {
            SerializedProcessingMachine serializedProcessingMachine = JsonConvert.DeserializeObject<SerializedProcessingMachine>(data);
            Inventory = MachineInventoryFactory.DeserializeMachineInventory(serializedProcessingMachine.SerializedMachineInventory, this);
            currentRecipe = RecipeSerializationFactory.Deserialize<ItemEnergyRecipe>(
                serializedProcessingMachine.SerializedGeneratorRecipe, 
                RecipeType.Machine
            );
            EnergyInventory = MachineInventoryFactory.DeserializeEnergyMachineInventory(serializedProcessingMachine.SerializedEnergyInventory, this);
            InventoryUpdate(0);
        }

        public override void tickUpdate()
        {
            if (ReferenceEquals(currentRecipe?.InputEnergy,null) || EnergyInventory.Energy == 0) return;

            ulong energyToUse = EnergyInventory.Energy < currentRecipe.EnergyCostPerTick ? EnergyInventory.Energy : currentRecipe.EnergyCostPerTick;
            EnergyInventory.Energy -= energyToUse;
            currentRecipe.InputEnergy -= energyToUse;
            if (currentRecipe.InputEnergy > 0) return;
            Inventory.TryOutputRecipe(currentRecipe);
        }

        

        public override void InventoryUpdate(int n) {
            if (currentRecipe != null)
            {
                bool complete = currentRecipe.InputEnergy == 0;
                if (complete)
                {
                    Inventory.TryOutputRecipe(currentRecipe);
                }
                return;
            }
            RecipeProcessorInstance recipeProcessorInstance = RecipeRegistry.GetProcessorInstance(tileEntityObject.RecipeProcessor);
            if (recipeProcessorInstance == null)
            {
                Debug.LogError("Null recipe processor instance");
                return;
            }
            currentRecipe = recipeProcessorInstance.GetRecipe<ItemEnergyRecipe>(Mode, Inventory.itemInputs, Inventory.fluidInputs);
        }

        public override float GetProgressPercent()
        {
            if (currentRecipe == null) return 0;
            return 1 - (float)currentRecipe.InputEnergy / currentRecipe.InitialCost;
        }

        public override void PlaceInitialize()
        {
            InitializeItemInventory();
            InitializeEnergyInventory();
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

