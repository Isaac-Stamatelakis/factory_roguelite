using System.Collections.Generic;
using Chunks;
using Conduits.Systems;
using Item.Slot;
using Items;
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
using Random = System.Random;

namespace TileEntity.Instances.Machine.Instances.Passive
{
    [CreateAssetMenu(fileName = "New Machine", menuName = "Tile Entity/Machine/Burner")]
    public class BurnerMachine : MachineObject
    {
        public ItemObject FuelRestriction;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
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
                uint duration = ItemRegistry.BurnableItemRegistry.GetBurnDuration(itemSlot.itemObject);
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
            if (RemainingDuration == 0) return;
            RemainingDuration--;
            if (RemainingDuration > 0) return;
            TryConsumeFuel();
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
        private readonly System.Random random = new Random();
        public BurnerMachineInstance(BurnerMachine tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public override void TickUpdate()
        {
            UpdateRecipe();
            BurnerFuelInventory.Tick();
            
            return;
            void UpdateRecipe()
            {
                if (currentRecipe == null) return;
            
                if (!BurnerFuelInventory.Active())
                {
                    float passiveSpeed = currentRecipe.PassiveSpeed;
                    if (passiveSpeed == 0 && currentRecipe.RemainingTicks < currentRecipe.InitalTicks) // Recipes with 0 passive speed 'cool off' when no fuel is burning reversing progress
                    {
                        currentRecipe.RemainingTicks++;
                        return;
                    }

                    double ran = random.NextDouble();
                    if (ran > passiveSpeed) return;
                }
                currentRecipe.RemainingTicks--;
                if (currentRecipe.RemainingTicks > 0) return;
                Inventory.TryOutputRecipe(currentRecipe);
            }
        }

        public override void OnRightClick()
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
            InventoryUpdate();
        }

        public override void InventoryUpdate()
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

        public override List<ItemSlot> GetDroppableItems()
        {
            List<ItemSlot> items = base.GetDroppableItems();
            items.AddRange(BurnerFuelInventory.BurnerSlots);
            return base.GetDroppableItems();
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
