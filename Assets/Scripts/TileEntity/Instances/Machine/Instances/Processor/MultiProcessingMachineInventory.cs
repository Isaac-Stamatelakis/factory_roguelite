using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace TileEntityModule.Instances.Machines {
    public class MultiProcessingMachineInventory : MachineInventory<AdvancedMachineInventoryLayout>, IInputMachineInventory, IOutputMachineInventory, IEnergyMachineInventory, IBatterySlotInventory
    {
        private InventoryCollection inputs;
        private InventoryCollection outputs;
        private InventoryCollection others;
        private int energy;
        private int mode;

        public InventoryCollection Inputs { get => inputs; set => inputs = value; }
        public InventoryCollection Outputs { get => outputs; set => outputs = value; }
        public InventoryCollection Others { get => others; set => others = value; }

        

       
        public static MultiProcessingMachineInventory deserialize(string data) {
            if (data == null) {
                return null;
            }
            try {
                SerializedMachineData serializedMachineData = Newtonsoft.Json.JsonConvert.DeserializeObject<SerializedMachineData>(data);
                MultiProcessingMachineInventory processingMachineInventory = new MultiProcessingMachineInventory();
                processingMachineInventory.inputs = InventoryCollectionHelper.deserialize(serializedMachineData.inputs);
                processingMachineInventory.outputs = InventoryCollectionHelper.deserialize(serializedMachineData.outputs);
                processingMachineInventory.others = InventoryCollectionHelper.deserialize(serializedMachineData.other);
                processingMachineInventory.energy = serializedMachineData.energy;
                processingMachineInventory.mode = serializedMachineData.mode;
                return processingMachineInventory;
            } catch (JsonSerializationException ex) {
                Debug.LogError(ex);
            }
            return null;
        }

        public override void display(MachineInventoryLayout layout,Transform parent)
        {
            throw new System.NotImplementedException();
        }

        [System.Serializable]
        private class SerializedMachineData {
            public int energy;
            public int mode;
            public string inputs;
            public string outputs;
            public string other;
            public SerializedMachineData(string inputs, string outputs,string other,int energy,int mode) {
                this.energy = energy;
                this.inputs = inputs;
                this.outputs = outputs;
                this.other = other;
                this.mode = mode;
            }
        }
    }
}

