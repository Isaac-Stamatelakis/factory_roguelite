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

namespace TileEntityModule.Instances.Machines {
    [CreateAssetMenu(fileName = "New Machine", menuName = "Tile Entity/Machine/Passive")]
    public class PassiveProcessor : TileEntity, ITickableTileEntity,  IClickableTileEntity, ISerializableTileEntity, IConduitInteractable, ISolidItemConduitInteractable, IFluidConduitInteractable, ISignalConduitInteractable, IProcessorTileEntity, IInventoryListener
    {
        [SerializeField] public PassiveRecipeProcessor recipeProcessor;
        [SerializeField] public Tier tier;
        [SerializeField] public GameObject machineUIPrefab;
        public StandardMachineInventoryLayout layout;
        private PassiveProcessorInventory inventory;
        private IPassiveRecipe currentRecipe;
        [Header("Can be set manually or by\nTools/TileEntity/SetPorts")]
        [SerializeField] public ConduitPortLayout conduitLayout;

        public override void initalize(Vector2Int tilePosition, TileBase tileBase, IChunk chunk)
        {
            base.initalize(tilePosition,tileBase, chunk);
            if (inventory == null) {
                inventory = PassiveMachineInventoryFactory.initalize(layout);
            }
        }

        public void onClick()   
        {
            recipeProcessor.displayTileEntity(inventory,recipeProcessor.name,this);
            
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
            ItemSlotHelper.insertListIntoInventory(inventory.FluidOutputs.Slots,fluidOutputs,tier.getFluidStorage());
            currentRecipe = null;
            inventoryUpdate();
        }

        public void inventoryUpdate() {
            if (currentRecipe != null) {
                return;
            }
            currentRecipe = recipeProcessor.GetPassiveRecipe(
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
            return conduitLayout;
        }


        public void insertSignal(int signal,Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        public int extractSignal(Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        public RecipeProcessor getRecipeProcessor()
        {
            return recipeProcessor;
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
            ItemSlotHelper.insertIntoInventory(inventory.FluidInputs.Slots, itemSlot, tier.getFluidStorage());
        }
    }

    
}

