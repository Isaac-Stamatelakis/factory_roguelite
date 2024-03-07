using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace TileEntityModule.Instances.Machines {
    /// <summary>
    /// The standard machine inventory with one item input, one item output, one fluid input, one fluid output, one energy inventory, and a mode
    /// </summary>
    public class StandardMachineInventory : MachineInventory<StandardMachineInventoryLayout>, IInputMachineInventory, IOutputMachineInventory, IEnergyMachineInventory, IBatterySlotInventory
    {
        private Inventory itemInputs;
        private Inventory itemOutputs;
        private Inventory fluidInputs;
        private Inventory fluidOutputs;
        private Inventory other;
        public int energy;
        public int mode;
        public int currentOperationEnergy;
        public int currentOperationCost;

        public Inventory ItemInputs { get => itemInputs; set => itemInputs = value; }
        public Inventory ItemOutputs { get => itemOutputs; set => itemOutputs = value; }
        public Inventory FluidInputs { get => fluidInputs; set => fluidInputs = value; }
        public Inventory FluidOutputs { get => fluidOutputs; set => fluidOutputs = value; }
        public Inventory Other { get => other; set => other = value; }
        public int Energy { get => energy; set => energy = value; }
        public int Mode { get => mode; set => mode = value; }

        public override void display(MachineInventoryLayout layout,Transform parent)
        {
            if (layout is not StandardMachineInventoryLayout standardLayout) {
                Debug.LogError("Invalid layout provided to display");
                return;
            }
            MachineUIFactory.initInventory(itemInputs.Slots,standardLayout.itemInputs,ItemState.Solid,"ItemInputs",parent);
            MachineUIFactory.initInventory(itemOutputs.Slots,standardLayout.itemOutputs,ItemState.Solid,"ItemOutputs",parent);
            MachineUIFactory.initInventory(fluidInputs.Slots,standardLayout.fluidInputs,ItemState.Fluid,"FluidInputs",parent);
            MachineUIFactory.initInventory(fluidOutputs.Slots,standardLayout.fluidOutputs,ItemState.Fluid,"FluidOutputs",parent);
        }
        public StandardMachineInventory(List<ItemSlot> itemInputs, List<ItemSlot> itemOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOutputs) {
            this.itemInputs = new Inventory(itemInputs);
            this.itemOutputs = new Inventory(itemOutputs);
            this.fluidInputs = new Inventory(fluidInputs);
            this.fluidOutputs = new Inventory(fluidOutputs);
        }
    }

    public static class StandardMachineInventoryFactory {
        public static StandardMachineInventory deserialize(string data) {
            if (data == null) {
                return null;
            }
            try {
                SerializedMachineData serializedMachineData = JsonConvert.DeserializeObject<SerializedMachineData>(data);
                StandardMachineInventory processingMachineInventory = new StandardMachineInventory(
                    ItemSlotFactory.deserialize(serializedMachineData.itemInputs),
                    ItemSlotFactory.deserialize(serializedMachineData.itemOutputs),
                    ItemSlotFactory.deserialize(serializedMachineData.fluidInputs),
                    ItemSlotFactory.deserialize(serializedMachineData.fluidOutputs)
                );
                processingMachineInventory.Energy = serializedMachineData.energy;
                processingMachineInventory.Mode = serializedMachineData.mode;
                return processingMachineInventory;
            } catch (JsonSerializationException ex) {
                Debug.LogError(ex);
            }
            return null;
            
        }

        public static StandardMachineInventory initalize(StandardMachineInventoryLayout machineInventoryLayout) {
            List<ItemSlot> itemInputs = null;
            if (machineInventoryLayout == null) {
                return null;
            }
            int count = machineInventoryLayout.itemInputs.Count;
            if (count > 0) {
                itemInputs = new List<ItemSlot>();
                for (int i = 0; i < count; i++) {
                    itemInputs.Add(null);
                }
            }
            List<ItemSlot> itemOutputs = null;
            count = machineInventoryLayout.itemOutputs.Count;
            if (count > 0) {
                itemOutputs = new List<ItemSlot>();
                for (int i = 0; i < count; i++) {
                    itemOutputs.Add(null);
                }
            }
            List<ItemSlot> fluidInputs = null;
            count = machineInventoryLayout.fluidInputs.Count;
            if (count > 0) {
                fluidInputs = new List<ItemSlot>();
                for (int i = 0; i < count; i++) {
                    fluidInputs.Add(null);
                }
            }
            List<ItemSlot> fluidOutputs = null;
            count = machineInventoryLayout.fluidOutputs.Count;
            if (count > 0) {
                fluidOutputs = new List<ItemSlot>();
                for (int i = 0; i < count; i++) {
                    fluidOutputs.Add(null);
                }
            }
            
            return new StandardMachineInventory(
                itemInputs,
                itemOutputs,
                fluidInputs,
                fluidOutputs
            );
        }

        public static string serialize(StandardMachineInventory processingMachineInventory) {
            SerializedMachineData serializedMachineData = new SerializedMachineData(
                itemInputs: InventoryFactory.serialize(processingMachineInventory.ItemInputs),
                itemOutputs: InventoryFactory.serialize(processingMachineInventory.ItemOutputs),
                fluidInputs: InventoryFactory.serialize(processingMachineInventory.FluidInputs),
                fluidOutputs: InventoryFactory.serialize(processingMachineInventory.FluidOutputs),
                other: InventoryFactory.serialize(processingMachineInventory.Other),
                energy: processingMachineInventory.Energy,
                mode: processingMachineInventory.Mode
            );
            return JsonConvert.SerializeObject(serializedMachineData);
        }
        

        private class SerializedMachineData {
            public int energy;
            public int mode;
            public string itemInputs;
            public string itemOutputs;
            public string fluidInputs;
            public string fluidOutputs;
            public string other;
            public SerializedMachineData(string itemInputs, string itemOutputs,string fluidInputs, string fluidOutputs, string other, int energy,int mode) {
                this.energy = energy;
                this.itemInputs = itemInputs;
                this.itemOutputs = itemOutputs;
                this.fluidInputs = fluidInputs;
                this.fluidOutputs = fluidOutputs;
                this.other = other;
                this.mode = mode;
            }
        }
    }
    
}

