using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecipeModule;
using Newtonsoft.Json;
using ItemModule.Inventory;

namespace TileEntityModule.Instances.Machines {
    public class PassiveProcessorInventory : MachineInventory<StandardMachineInventoryLayout> {
        private Inventory solidInputs;
        private Inventory solidOutputs;
        private Inventory fluidInputs;
        private Inventory fluidOutputs;
        private int mode;
        private IPassiveRecipe currentRecipe;
        private int remainingTicks;

        public Inventory SolidInputs { get => solidInputs; set => solidInputs = value; }
        public Inventory SolidOutputs { get => solidOutputs; set => solidOutputs = value; }
        public Inventory FluidInputs { get => fluidInputs; set => fluidInputs = value; }
        public Inventory FluidOutputs { get => fluidOutputs; set => fluidOutputs = value; }
        public int Mode { get => mode; set => mode = value; }
        public IPassiveRecipe CurrentRecipe { get => currentRecipe; set => currentRecipe = value; }
        public int RemainingTicks { get => remainingTicks; set => remainingTicks = value; }

        public override void display(InventoryLayout layout, Transform parent)
        {
            if (layout is not StandardMachineInventoryLayout standardLayout) {
                Debug.LogError("Invalid layout provided to display");
                return;
            }
            MachineUIFactory.initInventory(solidInputs.Slots,standardLayout.itemInputs,ItemState.Solid,"ItemInputs",parent);
            MachineUIFactory.initInventory(solidOutputs.Slots,standardLayout.itemOutputs,ItemState.Solid,"ItemOutputs",parent);
            MachineUIFactory.initInventory(fluidInputs.Slots,standardLayout.fluidInputs,ItemState.Fluid,"FluidInputs",parent);
            MachineUIFactory.initInventory(fluidOutputs.Slots,standardLayout.fluidOutputs,ItemState.Fluid,"FluidOutputs",parent);
        }
        public PassiveProcessorInventory(List<ItemSlot> itemInputs, List<ItemSlot> itemOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOutputs) {
            this.solidInputs = new Inventory(itemInputs);
            this.solidOutputs = new Inventory(itemOutputs);
            this.fluidInputs = new Inventory(fluidInputs);
            this.fluidOutputs = new Inventory(fluidOutputs);
        }
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
                itemInputs: InventoryFactory.serialize(processingMachineInventory.SolidInputs),
                itemOutputs: InventoryFactory.serialize(processingMachineInventory.SolidOutputs),
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
