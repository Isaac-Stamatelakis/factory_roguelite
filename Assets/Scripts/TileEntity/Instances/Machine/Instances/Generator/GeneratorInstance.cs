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
using TileEntity.Instances.Machine.Instances;
using TileEntity.Instances.Storage;
using TileEntity.Instances.WorkBenchs;
using UI;

namespace TileEntity.Instances.Machines
{
    
    public class GeneratorInstance : MachineInstance<Generator, GeneratorItemRecipe>
    {
        public GeneratorInstance(Generator tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
            
        }
        
        public override string Serialize()
        {
            SerializedGeneratorData serializedGeneratorData = new SerializedGeneratorData(
                Mode,
                TileEntityInventoryFactory.Serialize(Inventory.Content),
                MachineInventoryFactory.SerializedEnergyMachineInventory(MachineEnergyInventory),
                RecipeSerializationFactory.Serialize(currentRecipe, RecipeType.Generator)
            );
            return JsonConvert.SerializeObject(serializedGeneratorData);
        }

        public override void TickUpdate()
        {
            if (ReferenceEquals(currentRecipe,null)) {
                return;
            }

            EnergyInventory energyInventory = MachineEnergyInventory.EnergyInventory;
            ulong space = energyInventory.GetSpace();
            if (space < currentRecipe.EnergyOutputPerTick) return;
            
            energyInventory.Energy += currentRecipe.EnergyOutputPerTick;
            currentRecipe.RemainingTicks--;
            
            if (currentRecipe.RemainingTicks > 0) return;
            Inventory.TryOutputRecipe(currentRecipe);
        }
        

        public override void InventoryUpdate() {
            if (currentRecipe != null) {
                bool complete = currentRecipe.RemainingTicks == 0;
                if (complete)
                {
                    Inventory.TryOutputRecipe(currentRecipe);
                }
                return;
            }
            currentRecipe = RecipeRegistry.GetProcessorInstance(TileEntityObject.RecipeProcessor).GetRecipe<GeneratorItemRecipe>(
                Mode, 
            Inventory.Content.itemInputs,
            Inventory.Content.fluidInputs
            );
        }

        public override float GetProgressPercent()
        {
            if (currentRecipe == null) return 0;
            return (float)(1 - currentRecipe.RemainingTicks / currentRecipe.InitalTicks);
        }

        public override void PlaceInitialize()
        {
            InitializeItemInventory();
            InitializeEnergyInventory();
        }
        
        public override void Unserialize(string data)
        {
            SerializedGeneratorData serializedProcessingMachine = JsonConvert.DeserializeObject<SerializedGeneratorData>(data);
            Inventory = new MachineItemInventory(
                this,
                TileEntityInventoryFactory.Deserialize(serializedProcessingMachine.SerializedMachineInventory, GetMachineLayout())
            );
            MachineEnergyInventory = MachineInventoryFactory.DeserializeEnergyMachineInventory(serializedProcessingMachine.SerializedEnergyInventory, this);
            currentRecipe = RecipeSerializationFactory.Deserialize<GeneratorItemRecipe>(
                serializedProcessingMachine.SerializedGeneratorRecipe, 
                RecipeType.Generator
            );
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

