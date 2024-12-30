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
using TileEntity.Instances.WorkBenchs;
using UI;

namespace TileEntity.Instances.Machines
{
    
    public class GeneratorInstance : MachineInstance<Generator, GeneratorItemRecipe>
    {
        public GeneratorInstance(Generator tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
            
        }
        
        public override string serialize()
        {
            SerializedGeneratorData serializedGeneratorData = new SerializedGeneratorData(
                Mode,
                MachineInventoryFactory.SerializeItemMachineInventory(Inventory),
                MachineInventoryFactory.SerializedEnergyMachineInventory(EnergyInventory),
                RecipeSerializationFactory.Serialize(currentRecipe, RecipeType.Generator)
            );
            return JsonConvert.SerializeObject(serializedGeneratorData);
        }

        public override void tickUpdate()
        {
            if (ReferenceEquals(currentRecipe,null)) {
                return;
            }
            ulong space = EnergyInventory.GetSpace();
            if (space > currentRecipe.EnergyOutputPerTick)
            {
                EnergyInventory.Energy += currentRecipe.EnergyOutputPerTick;
                currentRecipe.RemainingTicks--;
            }
            else
            {
                EnergyInventory.Fill();
                double loss = (double)space/currentRecipe.EnergyOutputPerTick;
                currentRecipe.RemainingTicks -= loss;
            }
            
            if (!(currentRecipe.RemainingTicks <= 0)) return;
            currentRecipe = null;
            InventoryUpdate(0);

        }
        

        public override void InventoryUpdate(int n) {
            if (currentRecipe != null) {
                return;
            }
            currentRecipe = RecipeRegistry.GetProcessorInstance(TileEntityObject.RecipeProcessor).GetRecipe<GeneratorItemRecipe>(
                Mode, 
            Inventory.itemInputs,
            Inventory.fluidInputs
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
        
        public override void unserialize(string data)
        {
            SerializedGeneratorData serializedProcessingMachine = JsonConvert.DeserializeObject<SerializedGeneratorData>(data);
            Inventory = MachineInventoryFactory.DeserializeMachineInventory(serializedProcessingMachine.SerializedMachineInventory, this);
            EnergyInventory = MachineInventoryFactory.DeserializeEnergyMachineInventory(serializedProcessingMachine.SerializedEnergyInventory, this);
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

