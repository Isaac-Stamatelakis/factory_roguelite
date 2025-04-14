using System.Collections;
using System.Collections.Generic;
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
using TileEntity.Instances.WorkBenchs;

namespace TileEntity.Instances.Machines {
    public class PassiveProcessorInstance : MachineInstance<PassiveProcessor, PassiveItemRecipe>
    {
        public PassiveProcessorInstance(PassiveProcessor tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
            
        }
        
        public override string Serialize()
        {
            SerializedPassiveMachine serializedGeneratorData = new SerializedPassiveMachine(
                Mode,
                TileEntityInventoryFactory.Serialize(Inventory.Content),
                RecipeSerializationFactory.Serialize(currentRecipe, RecipeType.Passive)
            );
            return JsonConvert.SerializeObject(serializedGeneratorData);
        }

        public override void TickUpdate()
        {
            if (currentRecipe == null) {
                return;
            }

            if (currentRecipe.RemainingTicks > 0)
            {
                currentRecipe.RemainingTicks--;
                return;
            }
            Inventory.TryOutputRecipe(currentRecipe);
        }

        
        public override void InventoryUpdate() {
            if (currentRecipe != null) {
                return;
            }

            currentRecipe = RecipeRegistry.GetProcessorInstance(tileEntityObject.RecipeProcessor).GetRecipe<PassiveItemRecipe>(
                Mode, Inventory.Content.itemInputs, Inventory.Content.fluidInputs);
            
        }

        public override float GetProgressPercent()
        {
            if (currentRecipe == null)
            {
                return 0;
            }
            return 1-(float)(currentRecipe.RemainingTicks/currentRecipe.InitalTicks);
        }

        public override void PlaceInitialize()
        {
            InitializeItemInventory();
        }
        
        public override void Unserialize(string data)
        {
            SerializedPassiveMachine serializedProcessingMachine = JsonConvert.DeserializeObject<SerializedPassiveMachine>(data);
            Inventory = new MachineItemInventory(
                this, 
                TileEntityInventoryFactory.Deserialize(serializedProcessingMachine.SerializedMachineInventory,GetMachineLayout())
            );
            currentRecipe = RecipeSerializationFactory.Deserialize<PassiveItemRecipe>(
                serializedProcessingMachine.SerializedRecipe, 
                RecipeType.Passive
            );
        }
        
        private class SerializedPassiveMachine
        {
            public int Mode;
            public string SerializedMachineInventory;
            public string SerializedRecipe;

            public SerializedPassiveMachine(int mode, string serializedMachineInventory, string serializedRecipe)
            {
                Mode = mode;
                SerializedMachineInventory = serializedMachineInventory;
                SerializedRecipe = serializedRecipe;
            }
        }
    }

    
}

