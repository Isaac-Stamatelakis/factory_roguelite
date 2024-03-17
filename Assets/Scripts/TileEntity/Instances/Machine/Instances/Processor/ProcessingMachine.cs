using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GUIModule;
using ChunkModule;
using Newtonsoft.Json;
using RecipeModule.Transmutation;
using ConduitModule.Ports;
using UnityEngine.Tilemaps;
using RecipeModule.Processors;
using RecipeModule;

namespace TileEntityModule.Instances.Machines
{
    
    [CreateAssetMenu(fileName = "New Machine", menuName = "Tile Entity/Machine/Processing")]
    public class ProcessingMachine : TileEntity, ITickableTileEntity, IClickableTileEntity, ISerializableTileEntity, IConduitInteractable, ISolidItemConduitInteractable, IFluidConduitInteractable, IEnergyConduitInteractable, ISignalConduitInteractable, IProcessorTileEntity
    {
        
        [SerializeField] public AggregatedPoweredMachineProcessor processor;       
        [SerializeField] public Tier tier;
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
                inventory = StandardMachineInventoryFactory.initalize((StandardMachineInventoryLayout) processor.getInventoryLayout());
            }
        }

        public void onClick()   
        {
            processor.displayTileEntity(inventory,tier,name);
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
            ItemSlotHelper.insertListIntoInventory(inventory.ItemOutputs.Slots,recipeOut);
            currentRecipe = null;
        }

        public void inventoryUpdate() {
            if (currentRecipe != null) {
                return;
            }
            currentRecipe = processor.getRecipe(
                mode: inventory.Mode,
                solidInputs: inventory.ItemInputs.Slots,
                solidOutputs: inventory.ItemOutputs.Slots,
                fluidInputs: inventory.FluidInputs.Slots,
                fluidOutputs: inventory.FluidOutputs.Slots
            );
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
            if (itemSlot == null || itemSlot.itemObject == null) {
                return;
            }
            ItemSlotHelper.insertIntoInventory(inventory.ItemInputs.Slots,itemSlot);
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
            return processor;
        }
    }
}

