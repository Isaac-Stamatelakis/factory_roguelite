using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecipeModule;
using Newtonsoft.Json;
using Items.Inventory;

namespace TileEntityModule.Instances.Machines {
    public class PassiveProcessorInventory : StandardSolidAndFluidInventory {
        private int mode;
        private IPassiveRecipe currentRecipe;
        private int remainingTicks;

        public PassiveProcessorInventory(List<ItemSlot> itemInputs, List<ItemSlot> itemOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOutputs) : base(itemInputs, itemOutputs, fluidInputs, fluidOutputs)
        {
        }

        public int Mode { get => mode; set => mode = value; }
        public IPassiveRecipe CurrentRecipe { get => currentRecipe; set => currentRecipe = value; }
        public int RemainingTicks { get => remainingTicks; set => remainingTicks = value; }

    }
    public static class PassiveMachineInventoryFactory {
        public static PassiveProcessorInventory deserialize(string data) {
            if (data == null) {
                return null;
            }
            try {
                SerializedPassiveMachineData seralizedData = JsonConvert.DeserializeObject<SerializedPassiveMachineData>(data);
                PassiveProcessorInventory passiveProcessorInventory = new PassiveProcessorInventory(
                    ItemSlotFactory.deserialize(seralizedData.itemInputs),
                    ItemSlotFactory.deserialize(seralizedData.itemOutputs),
                    ItemSlotFactory.deserialize(seralizedData.fluidInputs),
                    ItemSlotFactory.deserialize(seralizedData.fluidOutputs)
                );
                passiveProcessorInventory.Mode = seralizedData.mode;
                passiveProcessorInventory.RemainingTicks = seralizedData.remainingTicks;
                return passiveProcessorInventory;
            } catch (JsonSerializationException ex) {
                Debug.LogError(ex);
            }
            return null;
            
        }

        public static PassiveProcessorInventory initalize(StandardMachineInventoryLayout machineInventoryLayout) {
            if (machineInventoryLayout == null) {
                return null;
            }
            List<ItemSlot> itemInputs = ItemSlotHelper.initEmptyInventory(machineInventoryLayout.itemInputs.Count);
            List<ItemSlot> itemOutputs = ItemSlotHelper.initEmptyInventory(machineInventoryLayout.itemOutputs.Count);
            List<ItemSlot> fluidInputs = ItemSlotHelper.initEmptyInventory(machineInventoryLayout.fluidInputs.Count);
            List<ItemSlot> fluidOutputs= ItemSlotHelper.initEmptyInventory(machineInventoryLayout.fluidOutputs.Count);
            
            return new PassiveProcessorInventory(
                itemInputs,
                itemOutputs,
                fluidInputs,
                fluidOutputs
            );
        }

        public static string serialize(PassiveProcessorInventory processingMachineInventory) {
            SerializedPassiveMachineData serializedMachineData = new SerializedPassiveMachineData(
                itemInputs: InventoryFactory.serialize(processingMachineInventory.ItemInputs),
                itemOutputs: InventoryFactory.serialize(processingMachineInventory.ItemOutputs),
                fluidInputs: InventoryFactory.serialize(processingMachineInventory.FluidInputs),
                fluidOutputs: InventoryFactory.serialize(processingMachineInventory.FluidOutputs),
                mode: processingMachineInventory.Mode,
                remainingTicks: processingMachineInventory.RemainingTicks,
                recipeID: null
            );
            return JsonConvert.SerializeObject(serializedMachineData);
        }
        

        private class SerializedPassiveMachineData {
            public int mode;
            public string itemInputs;
            public string itemOutputs;
            public string fluidInputs;
            public string fluidOutputs;
            public int remainingTicks;
            public string recipeID;
            public SerializedPassiveMachineData(string itemInputs, string itemOutputs,string fluidInputs, string fluidOutputs, int mode, int remainingTicks, string recipeID) {
                this.itemInputs = itemInputs;
                this.itemOutputs = itemOutputs;
                this.fluidInputs = fluidInputs;
                this.fluidOutputs = fluidOutputs;
                this.mode = mode;
                this.remainingTicks = remainingTicks;
                this.recipeID = recipeID;
            }
        }
    }
}
