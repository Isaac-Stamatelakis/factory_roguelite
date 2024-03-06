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
        private int currentRecipeEnergy;
        private int currentRecipeCost;

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

        private void initRecipe() {
            if (currentRecipe is not IEnergyConsumeRecipe energyConsumeRecipe) {
                Debug.LogError(name +  ": Processing Machine recieved recipe which doesn't consume energy");
                currentRecipe = null;
                return;
            }
            currentRecipeEnergy = energyConsumeRecipe.getTotalEnergyCost();
            currentRecipeCost = energyConsumeRecipe.getEnergyCostPerTick();

        }

        private void processRecipe() {
            if (inventory.Energy <= 0) {
                return;
            }
            int energyToUse = Mathf.Min(inventory.Energy, currentRecipeCost);
            inventory.Energy -= energyToUse;
            currentRecipeCost -= energyToUse;
            
            if (currentRecipeCost > 0) {
                return;
            }
            if (currentRecipeCost < 0) {
                inventory.Energy-=currentRecipeCost;
            }
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
                initRecipe();
                return;
            }
            currentRecipe = processTransmutableItemRecipe();
            if (currentRecipe != null) {
                initRecipe();
            }
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

        public int insertEnergy(int insertEnergy)
        {
            if (inventory.Energy >= tier.getEnergyStorage()) {
                return 0;
            }
            int sum = inventory.Energy+=insertEnergy;
            if (sum > tier.getEnergyStorage()) {
                inventory.Energy=tier.getEnergyStorage();
                Debug.Log(sum-tier.getEnergyStorage());
                return sum - tier.getEnergyStorage();
            }
            inventory.Energy = sum;
            return insertEnergy;
        }

        public void insertSignal(int signal)
        {
            throw new System.NotImplementedException();
        }

        public int extractSignal()
        {
            throw new System.NotImplementedException();
        }

        public ref int getEnergy()
        {
            return ref inventory.energy;
        }
    }
}

