using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GUIModule;
using ChunkModule;
using Newtonsoft.Json;
using RecipeModule.Transmutation;


namespace TileEntityModule.Instances.Machine
{
    
    [CreateAssetMenu(fileName = "New Machine", menuName = "Tile Entity/Machine/Machine")]
    public class Machine : TileEntity, ITickableTileEntity, IClickableTileEntity, ISerializableTileEntity, IConduitInteractable
    {
        public RecipeProcessor recipeProcessor;
        public int tier;
        public GameObject machineUIPrefab;
        public MachineLayout layout;
        private int energy;
        private List<ItemSlot> inputs;
        private List<ItemSlot> outputs;
        private List<ItemSlot> others;
        private IMachineRecipe currentRecipe;
        [Header("Can be set manually or by\nTools/TileEntity/SetPorts")]
        public ConduitPortDataCollection conduitPortData;
        private int mode;
        

        public void set(ConduitType conduitType, List<ConduitPortData> vects) {
            switch (conduitType) {
                case ConduitType.Item:
                    conduitPortData.itemPorts = vects;
                    break;
                case ConduitType.Fluid:
                    conduitPortData.fluidPorts = vects;
                    break;
                case ConduitType.Energy:
                    conduitPortData.signalPorts = vects;
                    break;
                case ConduitType.Signal:
                    conduitPortData.energyPorts = vects;
                    break;
            }
        }

        public override void initalize(Vector2Int tilePosition, IChunk chunk)
        {
            base.initalize(tilePosition,chunk);
            if (inputs == null) {
                inputs = ItemSlotFactory.createEmptyInventory(layout.inputs.Count);
            }
            if (outputs == null) {
                outputs = ItemSlotFactory.createEmptyInventory(layout.outputs.Count);
            }
            if (others == null) {
                others = ItemSlotFactory.createEmptyInventory(1);
            }
        }

        public void onClick()   
        {
            if (machineUIPrefab == null) {
                Debug.LogError("GUI GameObject for Machine:" + name + " null");
                return;
            }
            TileEntityGUIController tileEntityGUIController = GameObject.Find("TileEntityGUIController").GetComponent<TileEntityGUIController>();
            GameObject instantiatedUI = GameObject.Instantiate(machineUIPrefab);
            MachineUI machineUI = instantiatedUI.GetComponent<MachineUI>();
            if (machineUI == null) {
                Debug.LogError("Machine Gameobject doesn't have UI component");
                return;
            }
            machineUI.displayMachine(layout, inputs, outputs, others, name);
            tileEntityGUIController.setGUI(this,instantiatedUI);
        }
        

        public string serialize()
        {
            SerializedMachineData serializedMachineData = new SerializedMachineData(
                inputs: ItemSlotFactory.serializeList(inputs),
                outputs: ItemSlotFactory.serializeList(outputs),
                other: ItemSlotFactory.serializeList(others),
                energy: energy,
                mode: mode
            );
            return JsonConvert.SerializeObject(serializedMachineData);
        }

        public void tickUpdate()
        {
            inventoryUpdate(); // ONLY HERE FOR TESTING PURPOSES VERY INEFFICENT
            if (currentRecipe == null) {
                return;
            }
            processRecipe();

        }

        private void processRecipe() {
            List<ItemSlot> recipeOut = currentRecipe.getOutputs();
            for (int n = 0; n < recipeOut.Count; n++) {
                ItemSlot outputItem = recipeOut[n];
                for (int j = 0; j < outputs.Count; j++) {
                    ItemSlot outputSlot = outputs[j];
                    if (outputSlot == null || outputSlot.itemObject == null) {
                        outputs[j] = outputItem;
                        break;
                    }
                    if (outputSlot.itemObject.id == outputItem.itemObject.id) {
                        int sum = outputItem.amount + outputSlot.amount;
                        if (sum > Global.MaxSize) {
                            outputSlot.amount = Global.MaxSize;
                            outputItem.amount = sum - Global.MaxSize;
                        } else {
                            outputSlot.amount = sum;
                            break;
                        }
                    }
                }
            }
            currentRecipe = null;
        }

        public void inventoryUpdate() {
            if (currentRecipe != null) {
                return;
            }
            IRecipe recipe = recipeProcessor.getMatchingRecipe(inputs,outputs,mode);
            if (recipe == null) {
                return;
            }
            if (recipe is not IMachineRecipe) {
                Debug.LogError("Machine recieved recipe which was not machine recipe " + name);
                return;
            }
            currentRecipe = (IMachineRecipe) recipe;
            
        }
        public void unserialize(string data)
        {
            if (data == null) {
                return;
            }
            try {
                SerializedMachineData serializedMachineData = Newtonsoft.Json.JsonConvert.DeserializeObject<SerializedMachineData>(data);
                inputs = ItemSlotFactory.deserialize(serializedMachineData.inputs);
                outputs = ItemSlotFactory.deserialize(serializedMachineData.outputs);
                others = ItemSlotFactory.deserialize(serializedMachineData.other);
                energy = serializedMachineData.energy;
                mode = serializedMachineData.mode;
            } catch (JsonSerializationException ex) {
                Debug.LogError(ex);
            }
            
            

        }

        public ConduitPortDataCollection GetConduitPortData()
        {
            return conduitPortData;
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

