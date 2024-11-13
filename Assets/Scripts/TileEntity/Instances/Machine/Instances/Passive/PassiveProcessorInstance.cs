using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Newtonsoft.Json;
using RecipeModule.Transmutation;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using RecipeModule;
using Items.Inventory;

namespace TileEntityModule.Instances.Machines {
    public class PassiveProcessorInstance : TileEntityInstance<PassiveProcessor>, ITickableTileEntity,  IRightClickableTileEntity, ISerializableTileEntity, IConduitInteractable, ISolidItemConduitInteractable, IFluidConduitInteractable, ISignalConduitInteractable, IProcessorTileEntity, IInventoryListener
    {
        private PassiveProcessorInventory inventory;
        private IPassiveRecipe currentRecipe;

        public PassiveProcessorInstance(PassiveProcessor tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
            if (inventory == null) {
                inventory = PassiveMachineInventoryFactory.initalize(tileEntity.Layout);
            }
        }

        public void onRightClick()   
        {
            tileEntity.RecipeProcessor.displayTileEntity(inventory,tileEntity.name,this);
        }
        
        public string serialize()
        {
            return PassiveMachineInventoryFactory.serialize(inventory);
        }

        public void tickUpdate()
        {
            if (currentRecipe == null) {
                return;
            }
            processRecipe();

        }

        private void processRecipe() {
            if (inventory.RemainingTicks > 0) {
                inventory.RemainingTicks--;
                return;
            }
            List<ItemSlot> outputs = currentRecipe.getOutputs();
            List<ItemSlot> solidOutputs;
            List<ItemSlot> fluidOutputs;
            ItemSlotHelper.sortInventoryByState(outputs, out solidOutputs, out fluidOutputs);
            ItemSlotHelper.insertListIntoInventory(inventory.ItemOutputs.Slots,solidOutputs,Global.MaxSize);
            ItemSlotHelper.insertListIntoInventory(inventory.FluidOutputs.Slots,fluidOutputs,tileEntity.Tier.getFluidStorage());
            currentRecipe = null;
            inventoryUpdate(0);
        }

        public void inventoryUpdate(int n) {
            if (currentRecipe != null) {
                return;
            }
            currentRecipe = tileEntity.RecipeProcessor.GetPassiveRecipe(
                inventory.Mode,
                inventory.ItemInputs.Slots,
                inventory.FluidInputs.Slots,
                inventory.ItemOutputs.Slots,
                inventory.FluidOutputs.Slots
            );
            if (currentRecipe == null) {
                return;
            }
            inventory.RemainingTicks = currentRecipe.getRequiredTicks();
        }


        public void unserialize(string data)
        {
            inventory = PassiveMachineInventoryFactory.deserialize(data);
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return tileEntity.ConduitLayout;
        }


        public void insertSignal(bool signal,Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        public bool extractSignal(Vector2Int portPosition)
        {
            return false;
        }

        public RecipeProcessor getRecipeProcessor()
        {
            return tileEntity.RecipeProcessor;
        }

        public ItemSlot extractSolidItem(Vector2Int portPosition)
        {
            return ItemSlotHelper.extractFromInventory(inventory.ItemOutputs.Slots);
        }

        public void insertSolidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            ItemSlotHelper.insertIntoInventory(inventory.ItemInputs.Slots, itemSlot, Global.MaxSize);
        }

        public ItemSlot extractFluidItem(Vector2Int portPosition)
        {
            return ItemSlotHelper.extractFromInventory(inventory.FluidOutputs.Slots);
        }

        public void insertFluidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            ItemSlotHelper.insertIntoInventory(inventory.FluidInputs.Slots, itemSlot, tileEntity.Tier.getFluidStorage());
        }
    }

    
}

