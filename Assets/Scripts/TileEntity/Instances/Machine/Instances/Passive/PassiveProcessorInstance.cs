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
            /*
            if (inventory == null) {
                inventory = PassiveMachineInventoryFactory.initalize(tileEntity.Layout);
            }
            */
        }
        
        public override string serialize()
        {
            SerializedPassiveMachine serializedGeneratorData = new SerializedPassiveMachine(
                Mode,
                MachineInventoryFactory.SerializeItemMachineInventory(Inventory),
                RecipeSerializationFactory.Serialize(currentRecipe, RecipeType.PassiveItem)
            );
            return JsonConvert.SerializeObject(serializedGeneratorData);
        }

        public override void tickUpdate()
        {
            if (currentRecipe == null) {
                return;
            }
            processRecipe();

        }

        private void processRecipe() {
            /*
            if (inventory.RemainingTicks > 0) {
                inventory.RemainingTicks--;
                return;
            }
            List<ItemSlot> outputs = currentRecipe.getOutputs();
            ItemSlotHelper.sortInventoryByState(outputs, out var solidOutputs, out var fluidOutputs);
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
            currentRecipe = tileEntity.RecipeProcessor.GetPassiveRecipe(
                inventory.Mode,
                inventory.ItemInputs.Slots,
                inventory.FluidInputs.Slots,
                inventory.ItemOutputs.Slots,
                inventory.FluidOutputs.Slots
            );
            if (currentRecipe == null) {
                return;
            }
            inventory.RemainingTicks = currentRecipe.getRequiredTicks();
            */
        }

        public override float GetProgressPercent()
        {
            if (currentRecipe == null)
            {
                return 0;
            }
            return (float)(currentRecipe.RemainingTicks/currentRecipe.InitalTicks);
        }


        public override void unserialize(string data)
        {
            //inventory = PassiveMachineInventoryFactory.deserialize(data);
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

