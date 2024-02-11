using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GUIModule;
using ChunkModule;

namespace TileEntityModule.Instances.Machine
{
    [CreateAssetMenu(fileName = "New Machine", menuName = "Tile Entity/Machine/Machine")]
    public class Machine : TileEntity, ITickableTileEntity, IClickableTileEntity, ISerializableTileEntity
    {
        public RecipeProcessor recipeProcessor;
        public int tier;
        public GameObject machineUIPrefab;
        public MachineLayout layout;
        private int energy;
        private List<ItemSlot> inputs;
        private List<ItemSlot> outputs;
        private List<ItemSlot> others;
        private Recipe currentRecipe;

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
                energy: energy
            );
            return Newtonsoft.Json.JsonConvert.SerializeObject(serializedMachineData);
        }

        public void tickUpdate()
        {
            
        }

        public void unserialize(string data)
        {
            if (data == null) {
                return;
            }
            SerializedMachineData serializedMachineData = Newtonsoft.Json.JsonConvert.DeserializeObject<SerializedMachineData>(data);
            inputs = ItemSlotFactory.deserialize(serializedMachineData.inputs);
            outputs = ItemSlotFactory.deserialize(serializedMachineData.outputs);
            others = ItemSlotFactory.deserialize(serializedMachineData.other);
            energy = serializedMachineData.energy;

        }

        [System.Serializable]
        private class SerializedMachineData {
            public int energy;
            public string inputs;
            public string outputs;
            public string other;
            public SerializedMachineData(string inputs, string outputs,string other,int energy) {
                this.energy = energy;
                this.inputs = inputs;
                this.outputs = outputs;
                this.other = other;
            }
        }
    }
}

