using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule;
using Newtonsoft.Json;
using RecipeModule.Transmutation;
using ConduitModule.Ports;
using UnityEngine.Tilemaps;
using RecipeModule;

namespace TileEntityModule.Instances.Machines
{
    
    [CreateAssetMenu(fileName = "E~New Generator", menuName = "Tile Entity/Machine/Generator")]
    public class Generator : TileEntity, ITickableTileEntity, IRightClickableTileEntity, ISerializableTileEntity, IConduitInteractable, ISolidItemConduitInteractable, IFluidConduitInteractable, IEnergyConduitInteractable, ISignalConduitInteractable, IProcessorTileEntity, IInventoryListener
    {
        [SerializeField] public EnergyRecipeProcessor energyRecipeProcessor;
        [SerializeField] public Tier tier;
        private StandardMachineInventory inventory;
        private IEnergyProduceRecipe currentRecipe;
        [Header("Can be set manually or by\nTools/TileEntity/SetPorts")]
        [SerializeField] public ConduitPortLayout conduitLayout;
        private int remainingTicks;
        public override void initalize(Vector2Int tilePosition, TileBase tileBase, IChunk chunk)
        {
            base.initalize(tilePosition,tileBase, chunk);
            if (inventory == null) {
                inventory = StandardMachineInventoryFactory.initalize((StandardMachineInventoryLayout)energyRecipeProcessor.getInventoryLayout());
            }
        }

        public void onRightClick()   
        {
            energyRecipeProcessor.displayTileEntity(inventory,tier,energyRecipeProcessor.name,this);
        }
        

        public string serialize()
        {
            return StandardMachineInventoryFactory.serialize(inventory);
        }

        public void tickUpdate()
        {
            inventoryUpdate(0); // ONLY HERE FOR TESTING PURPOSES VERY INEFFICENT
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

        public void inventoryUpdate(int n) {
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


        public void insertSolidItem(ItemSlot itemSlot,Vector2Int portPosition)
        {
            ItemSlotHelper.insertIntoInventory(inventory.ItemInputs.Slots,itemSlot,Global.MaxSize);
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

        public ItemSlot extractSolidItem(Vector2Int portPosition)
        {
            return ItemSlotHelper.extractFromInventory(inventory.ItemOutputs.Slots);
        }

        public ItemSlot extractFluidItem(Vector2Int portPosition)
        {
            return ItemSlotHelper.extractFromInventory(inventory.FluidOutputs.Slots);
            
        }

        public void insertFluidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            ItemSlotHelper.insertIntoInventory(inventory.FluidInputs.Slots,itemSlot,tier.getFluidStorage());
        }
    }
}

