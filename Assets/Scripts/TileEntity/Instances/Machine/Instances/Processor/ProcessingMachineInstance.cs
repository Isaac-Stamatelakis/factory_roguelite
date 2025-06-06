using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Chunks;
using Newtonsoft.Json;
using Conduits.Ports;
using Item.Slot;
using UnityEngine.Tilemaps;
using RecipeModule;
using Items.Inventory;
using Recipe;
using Recipe.Data;
using Recipe.Processor;
using TileEntity.Instances.Machine.Instances;
using TileEntity.Instances.Storage;
using UI;

namespace TileEntity.Instances.Machines
{
    public interface IBatterySlotMachine
    {
        public List<ItemSlot> GetBatteryInventory();
    }
    public class ProcessingMachineInstance : MachineInstance<ProcessingMachine, ItemEnergyRecipe>, IBatterySlotMachine
    {
        public List<ItemSlot> BatteryInventory;
        public ProcessingMachineInstance(ProcessingMachine tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public override string Serialize()
        {
            SerializedProcessingMachine serializedProcessingMachine = new SerializedProcessingMachine(
                Mode,
                TileEntityInventoryFactory.Serialize(Inventory.Content),
                MachineInventoryFactory.SerializedEnergyMachineInventory(MachineEnergyInventory),
                RecipeSerializationFactory.Serialize(currentRecipe, RecipeType.Machine),
                ItemSlotFactory.serializeList(BatteryInventory)
            );
            return JsonConvert.SerializeObject(serializedProcessingMachine);
        }
        
        public override void Unserialize(string data)
        {
            SerializedProcessingMachine serializedProcessingMachine = JsonConvert.DeserializeObject<SerializedProcessingMachine>(data);
            Mode = serializedProcessingMachine.Mode;
            Inventory = new MachineItemInventory(
                this, 
                TileEntityInventoryFactory.Deserialize(serializedProcessingMachine.SerializedMachineInventory,GetMachineLayout())
            );
            currentRecipe = RecipeSerializationFactory.Deserialize<ItemEnergyRecipe>(
                serializedProcessingMachine.SerializedGeneratorRecipe, 
                RecipeType.Machine
            );
            MachineEnergyInventory = MachineInventoryFactory.DeserializeEnergyMachineInventory(serializedProcessingMachine.SerializedEnergyInventory, this);
            BatteryInventory = ItemSlotFactory.Deserialize(serializedProcessingMachine.SerializedBatteryInventory);
            InventoryUpdate();
        }

        public override void TickUpdate()
        {
            if (BatteryInventory?.Count > 0 && !ItemSlotUtils.IsItemSlotNull(BatteryInventory[0]))
            {
                // TODO draw energy from battery
            }

            EnergyInventory energyInventory = MachineEnergyInventory.EnergyInventory;
            if ((currentRecipe?.InputEnergy ?? 0) == 0 || energyInventory.Energy == 0) return;

            if (currentRecipe.InputEnergy > 0)
            {
                ulong energyToUse = energyInventory.Energy < currentRecipe.EnergyCostPerTick 
                    ? energyInventory.Energy 
                    : currentRecipe.EnergyCostPerTick;

                if (energyToUse < currentRecipe.InputEnergy)
                {
                    energyInventory.Energy -= energyToUse;
                    currentRecipe.InputEnergy -= energyToUse;
                    return;
                }
                energyInventory.Energy -= currentRecipe.InputEnergy;
                currentRecipe.InputEnergy = 0;
            }
            Inventory.TryOutputRecipe(currentRecipe);
        }

        

        public override void InventoryUpdate() {
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
            currentRecipe = recipeProcessorInstance.GetRecipe<ItemEnergyRecipe>(Mode, Inventory.Content.itemInputs, Inventory.Content.fluidInputs);
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
            BatteryInventory = new List<ItemSlot> { null };
        }
        
        public List<ItemSlot> GetBatteryInventory()
        {
            return BatteryInventory;
        }
        
        private class SerializedProcessingMachine
        {
            public int Mode;
            public string SerializedMachineInventory;
            public string SerializedEnergyInventory;
            public string SerializedGeneratorRecipe;
            public string SerializedBatteryInventory;

            public SerializedProcessingMachine(int mode, string serializedMachineInventory, string serializedEnergyInventory, string serializedGeneratorRecipe, string serializedBatteryInventory)
            {
                Mode = mode;
                SerializedMachineInventory = serializedMachineInventory;
                SerializedEnergyInventory = serializedEnergyInventory;
                SerializedGeneratorRecipe = serializedGeneratorRecipe;
                SerializedBatteryInventory = serializedBatteryInventory;
            }
        }

        
    }
}

