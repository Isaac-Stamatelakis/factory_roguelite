using System.Collections.Generic;
using Chunks;
using Conduits.Systems;
using Item.Slot;
using Newtonsoft.Json;
using Recipe;
using Recipe.Collection;
using Recipe.Data;
using Recipe.Processor;
using RecipeModule;
using TileEntity.Instances.Machine.UI;
using TileEntity.Instances.Machines;
using UI;
using UnityEngine;

namespace TileEntity.Instances.Machine.Instances.Passive
{
    [CreateAssetMenu(fileName = "New Machine", menuName = "Tile Entity/Machine/Burner")]
    public class BurnerMachine : MachineObject
    {
        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new BurnerMachineInstance(this, tilePosition, tileItem, chunk);
        }
    }

    public class BurnerFuelInventory
    {
        public List<ItemSlot> BurnerSlots;
        public BurnerMachineInstance Parent;
        public uint InitialDuration;
        public uint RemainingDuration;

        public BurnerFuelInventory(BurnerMachineInstance parent, List<ItemSlot> burnerSlots, uint initialDuration, uint remainingDuration)
        {
            BurnerSlots = burnerSlots;
            Parent = parent;
            InitialDuration = initialDuration;
            RemainingDuration = remainingDuration;
        }

        private void SetFuel(uint duration)
        {
            RemainingDuration = duration;
            InitialDuration = duration;
        }

        public void TryConsumeFuel()
        {
            foreach (ItemSlot itemSlot in BurnerSlots)
            {
                if (ItemSlotUtils.IsItemSlotNull(itemSlot)) continue;
                uint duration = RecipeRegistry.BurnableItemRegistry.GetBurnDuration(itemSlot.itemObject);
                if (duration == 0) continue;
                SetFuel(duration);
                itemSlot.amount--;
            }
        }

        public bool Active()
        {
            return RemainingDuration > 0;
        }

        public void Tick()
        {
            if (RemainingDuration > 0) RemainingDuration--;
            if (RemainingDuration == 0) Parent.InventoryUpdate(0);
        }

        public float GetBurnPercent()
        {
            if (InitialDuration == 0) return 0;
            return (float)RemainingDuration/InitialDuration;
        }
    }

    public interface IBurnerMachine
    {
        public BurnerFuelInventory GetBurnerInventory();
    }

    public class BurnerMachineInstance : MachineInstance<BurnerMachine, BurnerItemRecipe>, IBurnerMachine
    {
        public BurnerFuelInventory BurnerFuelInventory;
        public BurnerMachineInstance(BurnerMachine tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public override void tickUpdate()
        {
            BurnerFuelInventory.Tick();
            if (ReferenceEquals(currentRecipe,null) || currentRecipe.RemainingTicks <= 0) return;
            
            if (!BurnerFuelInventory.Active())
            {
                float passiveSpeed = currentRecipe.PassiveSpeed;
                bool coolOff = passiveSpeed == 0;
                if (coolOff) // Recipes with 0 passive speed 'cool off' when no fuel is burning reversing progress
                {
                    if (currentRecipe.RemainingTicks >= currentRecipe.InitalTicks) return;
                    currentRecipe.RemainingTicks += 1;
                    if (currentRecipe.RemainingTicks > currentRecipe.InitalTicks)
                    {
                        currentRecipe.RemainingTicks = currentRecipe.InitalTicks;
                    }
                    return;
                }
            }
            double tick = BurnerFuelInventory.Active() ? 1 : currentRecipe.PassiveSpeed;
            currentRecipe.RemainingTicks -= tick;
            if (currentRecipe.RemainingTicks > 0) return;
            Inventory.TryOutputRecipe(currentRecipe);
        }

        public override void onRightClick()
        {
            GameObject uiPrefab = TileEntityObject.RecipeProcessor.UIPrefab;
            GameObject ui = Object.Instantiate(uiPrefab);
            BurnerUIBase machineUI = ui.GetComponent<BurnerUIBase>();
            machineUI.DisplayTileEntityInstance(this);
            MainCanvasController.TInstance.DisplayUIWithPlayerInventory(ui);
        }

        public override string Serialize()
        {
            SerializedBurnerMachine serializedBurnerMachine = new SerializedBurnerMachine(
                Mode,
                TileEntityInventoryFactory.Serialize(Inventory.Content),
                MachineInventoryFactory.SerializeMachineBurnerInventory(BurnerFuelInventory),
                RecipeSerializationFactory.Serialize(currentRecipe, RecipeType.Burner)
            );
            return JsonConvert.SerializeObject(serializedBurnerMachine);
        }

        public override void Unserialize(string data)
        {
            SerializedBurnerMachine serializedBurnerMachine = JsonConvert.DeserializeObject<SerializedBurnerMachine>(data);
            Mode = serializedBurnerMachine.Mode;
            Inventory = new MachineItemInventory(
                this, 
                TileEntityInventoryFactory.Deserialize(serializedBurnerMachine.SerializedMachineInventory,GetMachineLayout())
            );
            currentRecipe = RecipeSerializationFactory.Deserialize<BurnerItemRecipe>(
                serializedBurnerMachine.SerializedBurnerRecipe, 
                RecipeType.Machine
            );
            BurnerFuelInventory = MachineInventoryFactory.DeserializeMachineBurnerInventory(this, serializedBurnerMachine.SerializedBurnerInventory);
            InventoryUpdate(0);
        }

        public override void InventoryUpdate(int n)
        {
            if (currentRecipe != null)
            {
                bool complete = currentRecipe.RemainingTicks <= 0;
                if (complete)
                {
                    Inventory.TryOutputRecipe(currentRecipe);
                }
                else if (!BurnerFuelInventory.Active())
                {
                    BurnerFuelInventory.TryConsumeFuel();
                }
                return;
            }
            currentRecipe = RecipeRegistry.GetProcessorInstance(tileEntityObject.RecipeProcessor).GetRecipe<BurnerItemRecipe>(
                Mode, 
                Inventory.Content.itemInputs, 
                Inventory.Content.fluidInputs
            );
            if (currentRecipe == null) return;
            if (!BurnerFuelInventory.Active()) BurnerFuelInventory.TryConsumeFuel();
        }

        public override float GetProgressPercent()
        {
            if (ReferenceEquals(currentRecipe, null) || currentRecipe.InitalTicks == 0) return 0;
            return 1-(float)currentRecipe.RemainingTicks / (float)currentRecipe.InitalTicks;
        }

        public override void PlaceInitialize()
        {
            BurnerFuelInventory = new BurnerFuelInventory(this,new List<ItemSlot>{null},0,0);
            Inventory = new MachineItemInventory(this, TileEntityInventoryFactory.Initialize(GetMachineLayout()));
        }

        public BurnerFuelInventory GetBurnerInventory()
        {
            return BurnerFuelInventory;
        }
        
        
        private class SerializedBurnerMachine
        {
            public int Mode;
            public string SerializedMachineInventory;
            public string SerializedBurnerInventory;
            public string SerializedBurnerRecipe;

            public SerializedBurnerMachine(int mode, string serializedMachineInventory, string serializedBurner, string serializedBurnerRecipe)
            {
                Mode = mode;
                SerializedMachineInventory = serializedMachineInventory;
                SerializedBurnerInventory = serializedBurner;
                SerializedBurnerRecipe = serializedBurner;
            }
        }
    }
}
