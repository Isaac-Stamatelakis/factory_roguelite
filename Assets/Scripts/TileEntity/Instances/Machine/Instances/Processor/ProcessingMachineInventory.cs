using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Items.Inventory;

namespace TileEntityModule.Instances.Machines {
    /// <summary>
    /// The standard machine inventory with one item input, one item output, one fluid input, one fluid output, one energy inventory, and a mode
    /// </summary>
    public class StandardMachineInventory : StandardSolidAndFluidInventory, IInputMachineInventory, IOutputMachineInventory, IEnergyMachineInventory, IBatterySlotInventory
    {
        private Inventory other;
        public int energy;
        public int mode;
        public int currentOperationEnergy;
        public int currentOperationCost;

        public StandardMachineInventory(List<ItemSlot> itemInputs, List<ItemSlot> itemOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOutputs) : base(itemInputs, itemOutputs, fluidInputs, fluidOutputs)
        {
        }

        public Inventory Other { get => other; set => other = value; }
        public int Energy { get => energy; set => energy = value; }
        public int Mode { get => mode; set => mode = value; }

        
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
            if (machineInventoryLayout == null) {
                return null;
            }
            List<ItemSlot> itemInputs = ItemSlotHelper.initEmptyInventory(machineInventoryLayout.itemInputs.Count);
            List<ItemSlot> itemOutputs = ItemSlotHelper.initEmptyInventory(machineInventoryLayout.itemOutputs.Count);
            List<ItemSlot> fluidInputs = ItemSlotHelper.initEmptyInventory(machineInventoryLayout.fluidInputs.Count);
            List<ItemSlot> fluidOutputs= ItemSlotHelper.initEmptyInventory(machineInventoryLayout.fluidOutputs.Count);
            
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

