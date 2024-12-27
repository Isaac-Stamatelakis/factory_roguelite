using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Newtonsoft.Json;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using RecipeModule;
using Items.Inventory;
using Recipe;
using Recipe.Data;
using Recipe.Processor;
using UI;

namespace TileEntity.Instances.Machines
{
    public class ProcessingMachineInstance : TileEntityInstance<ProcessingMachine>, ITickableTileEntity, 
        IRightClickableTileEntity, ISerializableTileEntity, IConduitTileEntityAggregator, ISignalConduitInteractable, IInventoryListener
    {
        private int mode;
        private MachineItemInventory inventory;
        private MachineEnergyInventory energyInventory;
        private ItemEnergyRecipe currentRecipe;
        public ProcessingMachineInstance(ProcessingMachine tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
            /*
            if (inventory == null) {
                if (tileEntity.Processor == null) {
                    Debug.LogWarning($"Tile entity {tileEntity.name} has no processor");
                    return;
                }
                inventory = StandardMachineInventoryFactory.initalize(tileEntity.Layout);
            }
            */
        }

        public void onRightClick()
        {
            GameObject uiPrefab = TileEntityObject.Processor.UIPrefab;
            GameObject ui = GameObject.Instantiate(uiPrefab);
            CanvasController.Instance.DisplayObject(ui);
        }
        

        public string serialize()
        {
            SerializedProcessingMachine serializedProcessingMachine = new SerializedProcessingMachine(
                mode,
                MachineInventoryFactory.SerializeItemMachineInventory(inventory),
                MachineInventoryFactory.SerializedEnergyMachineInventory(energyInventory),
                RecipeSerializationFactory.Serialize(currentRecipe, RecipeType.EnergyItem)
            );
            return JsonConvert.SerializeObject(serializedProcessingMachine);
        }
        
        public void unserialize(string data)
        {
            //inventory = StandardMachineInventoryFactory.deserialize(data);
        }

        public void tickUpdate()
        {
            if (currentRecipe == null) {
                return;
            }
            ProcessRecipe();
        }

        private void ProcessRecipe() {
            /*
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
            List<ItemSlot> recipeOut = currentRecipe.Outputs;
            ItemSlotHelper.sortInventoryByState(recipeOut, out var solidOutputs, out var fluidOutputs);
            ItemSlotHelper.InsertListIntoInventory(inventory.ItemOutputs.Slots,solidOutputs,Global.MaxSize);
            ItemSlotHelper.InsertListIntoInventory(inventory.FluidOutputs.Slots,fluidOutputs,tileEntity.Tier.GetFluidStorage());
            currentRecipe = null;
            inventoryUpdate(0);
            */
        }

        public void inventoryUpdate(int n) {
            /*
            if (currentRecipe != null) {
                return;
            }
            currentRecipe = RecipeRegistry.GetInstance().LookUpRecipe(inventory.ItemInputs.Slots, inventory.FluidInputs.Slots);
            */
            
        }
        
        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.ConduitLayout;
        }

        public IConduitInteractable GetConduitInteractable(ConduitType conduitType)
        {
            switch (conduitType)
            {
                case ConduitType.Energy:
                    return energyInventory;
                case ConduitType.Item:
                case ConduitType.Fluid:
                    return inventory;
                case ConduitType.Signal:
                    return this;
                default:
                    return null;
            }
        }

        public bool ExtractSignal(Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        public void InsertSignal(bool active, Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        private class SerializedProcessingMachine
        {
            public int Mode;
            public string SerializedMachineInventory;
            public string SerializedEnergyInventory;
            public string SerializedGeneratorRecipe;

            public SerializedProcessingMachine(int mode, string serializedMachineInventory, string serializedEnergyInventory, string serializedGeneratorRecipe)
            {
                Mode = mode;
                SerializedMachineInventory = serializedMachineInventory;
                SerializedEnergyInventory = serializedEnergyInventory;
                SerializedGeneratorRecipe = serializedGeneratorRecipe;
            }
        }
    }
}

