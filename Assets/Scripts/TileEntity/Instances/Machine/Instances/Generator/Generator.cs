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
    public class Generator : TileEntity, ITickableTileEntity, IClickableTileEntity, ISerializableTileEntity, IConduitInteractable, ISolidItemConduitInteractable, IFluidConduitInteractable, IEnergyConduitInteractable, ISignalConduitInteractable, IProcessorTileEntity
    {
        [SerializeField] public EnergyRecipeProcessor energyRecipeProcessor;
        [SerializeField] public Tier tier;
        [SerializeField] public GameObject machineUIPrefab;
        public StandardMachineInventoryLayout layout;
        private StandardMachineInventory inventory;
        private IEnergyProduceRecipe currentRecipe;
        [Header("Can be set manually or by\nTools/TileEntity/SetPorts")]
        [SerializeField] public ConduitPortLayout conduitLayout;
        private int remainingTicks;
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
            if (remainingTicks > 0) {
                if (inventory.Energy < tier.getEnergyStorage()) {
                    remainingTicks--;
                    inventory.Energy += currentRecipe.getEnergyPerTick();
                }
                
            }
            if (remainingTicks <= 0) {
                currentRecipe = null;
            }
        }

        public void inventoryUpdate() {
            if (currentRecipe != null) {
                return;
            }
            currentRecipe = processEnergyRecipes();
            if (currentRecipe == null) {
                return;
            }
            remainingTicks = currentRecipe.getLifespan();
            
        }

        private IEnergyProduceRecipe processEnergyRecipes() {
            if (energyRecipeProcessor == null) {
                return null;
            }
            IEnergyProduceRecipe recipe = energyRecipeProcessor.getEnergyRecipe(
                mode: inventory.Mode,
                solidInputs: inventory.ItemInputs.Slots,
                solidOutputs: inventory.ItemOutputs.Slots,
                fluidInputs: inventory.FluidInputs.Slots,
                fluidOutputs: inventory.FluidOutputs.Slots
            );
            if (recipe == null) {
                return null;
            }
            if (recipe is not IEnergyProduceRecipe energyProduceRecipe) {
                Debug.LogError("Machine '" + name + "' Reciped assigned to machine");
                return null;
            }
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

        public ItemSlot extractItem(Vector2Int portPosition)
        {
            foreach (ItemSlot itemSlot in inventory.ItemOutputs.Slots) {
                if (itemSlot != null && itemSlot.itemObject != null) {
                    return itemSlot;
                }
            }
            return null;
        }

        public void insertItem(ItemSlot itemSlot,Vector2Int portPosition)
        {
            List<ItemSlot> inputs = inventory.ItemInputs.Slots;
            for (int i = 0; i < inputs.Count; i++) {
                ItemSlot inputSlot = inputs[i];
                if (inputSlot == null || inputSlot.itemObject == null) {
                    inputs[i] = new ItemSlot(itemSlot.itemObject,itemSlot.amount,itemSlot.tags);
                    itemSlot.amount=0;
                    return;
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
                return;
            }
        }

        public ItemSlot extractFluid(Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        public bool insertFluid(ItemSlot itemSlot,Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        public int insertEnergy(int insertEnergy,Vector2Int portPosition)
        {
            inventory.Energy += insertEnergy;
            return 0;
        }

        public void insertSignal(int signal,Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        public int extractSignal(Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

    

        public ref int getEnergy(Vector2Int portPosition)
        {
            return ref inventory.energy;
        }

        

        public RecipeProcessor getRecipeProcessor()
        {
            return energyRecipeProcessor;
        }
    }
}

