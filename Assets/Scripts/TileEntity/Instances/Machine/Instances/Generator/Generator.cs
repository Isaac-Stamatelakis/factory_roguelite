using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GUIModule;
using ChunkModule;
using Newtonsoft.Json;
using RecipeModule.Transmutation;
using ConduitModule.Ports;
using UnityEngine.Tilemaps;
using RecipeModule;

namespace TileEntityModule.Instances.Machines
{
    
    [CreateAssetMenu(fileName = "E~New Generator", menuName = "Tile Entity/Machine/Generator")]
    public class Generator : TileEntity, ITickableTileEntity, IClickableTileEntity, ISerializableTileEntity, IConduitInteractable, ISolidItemConduitInteractable, IFluidConduitInteractable, IEnergyConduitInteractable, ISignalConduitInteractable
    {
        public List<IRecipeProcessor> recipeProcessors;
        public Tier tier;
        public GameObject machineUIPrefab;
        public MachineInventoryLayout layout;
        private int energy;
        private List<ItemSlot> inputs;
        private List<ItemSlot> others;
        private IMachineRecipe currentRecipe;
        [Header("Can be set manually or by\nTools/TileEntity/SetPorts")]
        public ConduitPortLayout conduitLayout;
        private int mode;
        

        public override void initalize(Vector2Int tilePosition, TileBase tileBase, IChunk chunk)
        {
            base.initalize(tilePosition,tileBase, chunk);
            
        }

        public void onClick()   
        {
            if (machineUIPrefab == null) {
                Debug.LogError("GUI GameObject for Machine:" + name + " null");
                return;
            }
            GameObject instantiatedUI = GameObject.Instantiate(machineUIPrefab);
            ProcessMachineUI machineUI = instantiatedUI.GetComponent<ProcessMachineUI>();
            if (machineUI == null) {
                Debug.LogError("Machine Gameobject doesn't have UI component");
                return;
            }
            //machineUI.displayMachine(layout, inputs, null, others, name);
            //GlobalUIContainer.getInstance().getUiController().setGUI(instantiatedUI);
        }
        

        public string serialize()
        {
            SerializedMachineData serializedMachineData = new SerializedMachineData(
                inputs: ItemSlotFactory.serializeList(inputs),
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
            
        }

        public void inventoryUpdate() {
            /*
            if (currentRecipe != null) {
                return;
            }
            //IRecipe recipe = recipeProcessor.getMatchingRecipe(inputs,outputs,mode);
            if (recipe == null) {
                return;
            }
            if (recipe is not IMachineRecipe) {
                Debug.LogError("Machine recieved recipe which was not machine recipe " + name);
                return;
            }
            currentRecipe = (IMachineRecipe) recipe;
            */
            
        }
        public void unserialize(string data)
        {
            if (data == null) {
                return;
            }
            try {
                SerializedMachineData serializedMachineData = Newtonsoft.Json.JsonConvert.DeserializeObject<SerializedMachineData>(data);
                inputs = ItemSlotFactory.deserialize(serializedMachineData.inputs);
                others = ItemSlotFactory.deserialize(serializedMachineData.other);
                energy = serializedMachineData.energy;
                mode = serializedMachineData.mode;
            } catch (JsonSerializationException ex) {
                Debug.LogError(ex);
            }
            
            

        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return conduitLayout;
        }

        public ItemSlot insertItem(ItemSlot itemSlot)
        {
            for (int i = 0; i < inputs.Count; i++) {
                ItemSlot inputSlot = inputs[i];
                if (inputSlot == null || inputSlot.itemObject == null) {
                    inputs[i] = new ItemSlot(itemSlot.itemObject,itemSlot.amount,itemSlot.nbt);
                    itemSlot.amount=0;
                    return inputs[i];
                }
                if (inputSlot.itemObject.id != itemSlot.itemObject.id) {
                    continue;
                }
                if (inputSlot.amount >= Global.MaxSize) {
                    continue;
                }
                // Success
                int sum = inputSlot.amount + itemSlot.amount;
                if (sum > Global.MaxSize) {
                    itemSlot.amount = sum - Global.MaxSize;
                    inputSlot.amount = Global.MaxSize;
                } else {
                    inputSlot.amount = sum;
                    itemSlot.amount = 0;
                }
                return inputSlot;
            }
            return null;
        }

        public ItemSlot extractFluid()
        {
            throw new System.NotImplementedException();
        }

        public bool insertFluid(ItemSlot itemSlot)
        {
            throw new System.NotImplementedException();
        }
        public int extractEnergy(int extractionRate)
        {
            Debug.LogError("Tried to extract energy from processing machine");
            throw new System.NotImplementedException();
        }

        public void insertEnergy(int insertEnergy)
        {
            energy += insertEnergy;
        }

        public void insertSignal(int signal)
        {
            throw new System.NotImplementedException();
        }

        public int extractSignal()
        {
            throw new System.NotImplementedException();
        }

        public void set(ConduitType conduitType, List<TileEntityPort> vects)
        {
            throw new System.NotImplementedException();
        }

        public ItemSlot extractItem()
        {
            Debug.LogError("Attempted to extract item from generator");
            throw new System.NotImplementedException();
        }

        [System.Serializable]
        private class SerializedMachineData {
            public int energy;
            public int mode;
            public string inputs;
            public string other;
            public SerializedMachineData(string inputs,string other,int energy,int mode) {
                this.energy = energy;
                this.inputs = inputs;
                this.other = other;
                this.mode = mode;
            }
        }
    }
}

