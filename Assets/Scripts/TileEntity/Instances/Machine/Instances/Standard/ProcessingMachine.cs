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
    
    [CreateAssetMenu(fileName = "New Machine", menuName = "Tile Entity/Machine/Processing")]
    public class ProcessingMachine : TileEntity, ITickableTileEntity, IClickableTileEntity, ISerializableTileEntity, IConduitInteractable, ISolidItemConduitInteractable, IFluidConduitInteractable, IEnergyConduitInteractable, ISignalConduitInteractable
    {
        
        [SerializeField] public ItemRecipeProcessor itemRecipeProcessor;
        [SerializeField] public TransmutableRecipeProcessor transmutableRecipeProcessor;
        
        [SerializeField] public Tier tier;
        [SerializeField] public GameObject machineUIPrefab;
        public StandardMachineInventoryLayout layout;
        private StandardMachineInventory inventory;
        private IMachineRecipe currentRecipe;
        [Header("Can be set manually or by\nTools/TileEntity/SetPorts")]
        [SerializeField] public ConduitPortLayout conduitLayout;

        public override void initalize(Vector2Int tilePosition, TileBase tileBase, IChunk chunk)
        {
            base.initalize(tilePosition,tileBase, chunk);
            if (inventory == null) {
                inventory = StandardMachineInventoryFactory.initalize(layout);
            }
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
            machineUI.displayMachine(layout, inventory, name, tier);
            GlobalUIContainer.getInstance().getUiController().setGUI(instantiatedUI);
        }
        

        public string serialize()
        {
            return StandardMachineInventoryFactory.serialize(inventory);
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
                for (int j = 0; j < inventory.ItemOutputs.Slots.Count; j++) {
                    ItemSlot outputSlot = inventory.ItemOutputs.Slots[j];
                    if (outputSlot == null || outputSlot.itemObject == null) {
                        inventory.ItemOutputs.Slots[j] = outputItem;
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
            currentRecipe = processItemRecipes();
            if (currentRecipe != null) {
                return;
            }
            currentRecipe = processTransmutableItemRecipe();
        }

        private IMachineRecipe processItemRecipes() {
            if (itemRecipeProcessor == null) {
                return null;
            }
            IItemRecipe recipe = itemRecipeProcessor.getItemRecipe(
                mode: inventory.Mode,
                solidInputs: inventory.ItemInputs.Slots,
                solidOutputs: inventory.ItemOutputs.Slots,
                fluidInputs: inventory.FluidInputs.Slots,
                fluidOutputs: inventory.FluidOutputs.Slots
            );
            if (recipe is not IMachineRecipe machineRecipe) {
                Debug.LogError("Machine '" + name + "' Reciped assigned to machine");
                return null;
            }
            return machineRecipe;
            
        }

        private IMachineRecipe processTransmutableItemRecipe() {
            if (transmutableRecipeProcessor == null) {
                return null;
            }
            TransmutableRecipe recipe = transmutableRecipeProcessor.getValidRecipe(
                mode: inventory.Mode,
                solidInputs: inventory.ItemInputs.Slots,
                solidOutputs: inventory.ItemOutputs.Slots,
                fluidInputs: inventory.FluidInputs.Slots,
                fluidOutputs: inventory.FluidOutputs.Slots
            );
            return recipe;
            
        }
        public void unserialize(string data)
        {
            inventory = StandardMachineInventoryFactory.deserialize(data);
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return conduitLayout;
        }

        public ItemSlot extractItem()
        {
            foreach (ItemSlot itemSlot in inventory.ItemOutputs.Slots) {
                if (itemSlot != null && itemSlot.itemObject != null) {
                    return itemSlot;
                }
            }
            return null;
        }

        public ItemSlot insertItem(ItemSlot itemSlot)
        {
            List<ItemSlot> inputs = inventory.ItemInputs.Slots;
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
            inventory.Energy += insertEnergy;
        }

        public void insertSignal(int signal)
        {
            throw new System.NotImplementedException();
        }

        public int extractSignal()
        {
            throw new System.NotImplementedException();
        }

        
    }
}

