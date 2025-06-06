using System;
using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using Item.Slot;
using UnityEngine;
using Items.Inventory;
using Newtonsoft.Json;
using Recipe.Data;
using RecipeModule;
using TileEntity.Instances.Machine;
using TileEntity.Instances.Machine.Instances.Passive;
using TileEntity.Instances.Machine.UI;
using TileEntity.Instances.Storage;

namespace TileEntity {
    public class TileEntityInventory : IDroppableInventory {
        public List<ItemSlot> itemInputs;
        public List<ItemSlot> itemOutputs;
        public List<ItemSlot> fluidInputs;
        public List<ItemSlot> fluidOutputs;

        public TileEntityInventory(List<ItemSlot> itemInputs, List<ItemSlot> itemOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOutputs)
        {
            this.itemInputs = itemInputs;
            this.itemOutputs = itemOutputs;
            this.fluidInputs = fluidInputs;
            this.fluidOutputs = fluidOutputs;
        }
        
        public ItemSlot ExtractFromInventory(ItemState state, ItemFilter itemFilter)
        {
            return state switch
            {
                ItemState.Solid => ItemSlotUtils.ExtractFromInventory(itemOutputs),
                ItemState.Fluid => ItemSlotUtils.ExtractFromInventory(fluidOutputs),
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        public List<ItemSlot> GetDroppableItems()
        {
            List<ItemSlot> dropList = new List<ItemSlot>();
            dropList.AddRange(itemInputs);
            dropList.AddRange(itemOutputs);
            return dropList;
        }
    }
    public class MachineItemInventory : IItemConduitInteractable
    {
        public MachineItemInventory(IMachineInstance parent, TileEntityInventory tileEntityInventory)
        {
            this.parent = parent;
            this.tileEntityInventory = tileEntityInventory;
        }

        public TileEntityInventory Content => tileEntityInventory;

        private TileEntityInventory tileEntityInventory;
        
        protected IMachineInstance parent;
        public IMachineInstance Parent => parent;
        
        public ItemSlot ExtractItem(ItemState state, Vector2Int portPosition, ItemFilter filter)
        {
            ItemSlot extracted = tileEntityInventory.ExtractFromInventory(state,filter);
            if (ItemSlotUtils.IsItemSlotNull(extracted)) return null;
            parent.InventoryUpdate();
            return extracted;
        }
        

        public void InsertItem(ItemState state, ItemSlot toInsert, Vector2Int portPosition)
        {
            switch (state)
            {
                case ItemState.Solid:
                    if(ItemSlotUtils.InsertIntoInventory(tileEntityInventory.itemInputs, toInsert, Global.MAX_SIZE)) parent.InventoryUpdate();;
                    break;
                case ItemState.Fluid:
                    Tier tier = Tier.Basic;
                    if (parent.GetTileEntity() is ITieredTileEntity tieredTileEntity)
                    {
                        tier = tieredTileEntity.GetTier();
                    }
                    if(ItemSlotUtils.InsertIntoInventory(tileEntityInventory.fluidInputs, toInsert, tier.GetFluidStorage())) parent.InventoryUpdate();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public void TryOutputRecipe(ItemRecipe itemRecipe)
        {
            ItemSlotUtils.InsertInventoryIntoInventory(tileEntityInventory.itemOutputs, itemRecipe.SolidOutputs, Global.MAX_SIZE);
            ItemSlotUtils.InsertInventoryIntoInventory(tileEntityInventory.fluidOutputs,itemRecipe.FluidOutputs , 64000); // TODO change from 64000 to vary with tier
            bool recipeConsumed = RecipeUtils.OutputsUsed(itemRecipe);
            if (!recipeConsumed) return;
            parent.ResetRecipe();
            parent.InventoryUpdate();
        }
    }

    public interface IDroppableInventory
    {
        public List<ItemSlot> GetDroppableItems();
    }

    public class MachineEnergyInventory :  IEnergyPortTileEntityAggregator
    {
        public EnergyInventory EnergyInventory;
        public MachineEnergyInventory(ulong energy, IMachineInstance parent)
        {
            Tier tier = Tier.Basic;
            if (parent.GetTileEntity() is ITieredTileEntity tieredTileEntity)
            {
                tier = tieredTileEntity.GetTier();
            }
            
            EnergyInventory = new CallBackEnergyInventory(energy, tier.GetEnergyStorage(), parent.InventoryUpdate);
        }
        
        public IEnergyConduitInteractable GetEnergyConduitInteractable()
        {
            return EnergyInventory;
        }
    }
    public static class TileEntityInventoryFactory {
        public static TileEntityInventory Deserialize(string json, TileEntityLayoutObject layoutObject)
        {
            if (json == null)
            {
                return Initialize(layoutObject);
            }
            try
            {
                SerializedTileEntityInventory sInventory = JsonConvert.DeserializeObject<SerializedTileEntityInventory>(json);
                
                var solidInputs = ItemSlotFactory.Deserialize(sInventory.SerializedSolidInputs);
                var solidOutputs = ItemSlotFactory.Deserialize(sInventory.SerializedSolidOutputs);
                var fluidInputs = ItemSlotFactory.Deserialize(sInventory.SerializedFluidInputs);
                var fluidOutputs = ItemSlotFactory.Deserialize(sInventory.SerializedFluidOutputs);
                return new TileEntityInventory(solidInputs, solidOutputs, fluidInputs, fluidOutputs);;
            }
            catch (JsonSerializationException e)
            {
                Debug.LogWarning(e);
                return Initialize(layoutObject);
            }
        }
        
        public static string Serialize(TileEntityInventory inventory)
        {
            try
            {
                SerializedTileEntityInventory sInventory = new SerializedTileEntityInventory(
                    ItemSlotFactory.serializeList(inventory.itemInputs),
                    ItemSlotFactory.serializeList(inventory.itemOutputs),
                    ItemSlotFactory.serializeList(inventory.fluidInputs),
                    ItemSlotFactory.serializeList(inventory.fluidOutputs)
                );
                
                return JsonConvert.SerializeObject(sInventory);
            }
            catch (NullReferenceException e)
            {
                Debug.LogWarning("Could not serialize inventory: " + e);
                return null;
            }
        }
        
        public static TileEntityInventory Initialize(TileEntityLayoutObject layoutObject)
        {   
            return new TileEntityInventory(
                ItemSlotFactory.createEmptyInventory(layoutObject.SolidInputs.GetIntSize()),
                ItemSlotFactory.createEmptyInventory(layoutObject.SolidOutputs.GetIntSize()),
                ItemSlotFactory.createEmptyInventory(layoutObject.FluidInputs.GetIntSize()),
                ItemSlotFactory.createEmptyInventory(layoutObject.FluidOutputs.GetIntSize())
            );
        }
        private class SerializedTileEntityInventory
        {
            public string SerializedSolidInputs;
            public string SerializedSolidOutputs;
            public string SerializedFluidInputs;
            public string SerializedFluidOutputs;

            public SerializedTileEntityInventory(string serializedSolidInputs, string serializedSolidOutputs, string serializedFluidInputs, string serializedFluidOutputs)
            {
                SerializedSolidInputs = serializedSolidInputs;
                SerializedSolidOutputs = serializedSolidOutputs;
                SerializedFluidInputs = serializedFluidInputs;
                SerializedFluidOutputs = serializedFluidOutputs;
            }
        }
    }
    public static class MachineInventoryFactory
    {
        public static string SerializedEnergyMachineInventory(MachineEnergyInventory machineInventory)
        {
            return machineInventory == null ? null : JsonConvert.SerializeObject(machineInventory.EnergyInventory.Energy);
        }

        public static string SerializeMachineBurnerInventory(BurnerFuelInventory burnerFuelInventory)
        {
            if (burnerFuelInventory == null) return null;

            try
            {
                SerializedBurnerData serializedBurnerData = new SerializedBurnerData(
                    ItemSlotFactory.serializeList(burnerFuelInventory.BurnerSlots),
                    burnerFuelInventory.InitialDuration,
                    burnerFuelInventory.RemainingDuration
                );
                return JsonConvert.SerializeObject(serializedBurnerData);
            }
            catch (NullReferenceException e)
            {
                Debug.LogWarning("Could not serialize burner inventory: " + e);
                return null;
            }
        }

        public static BurnerFuelInventory DeserializeMachineBurnerInventory(BurnerMachineInstance parent, string json)
        {
            if (json == null) return null;
            try
            {
                SerializedBurnerData serializedBurnerData = JsonConvert.DeserializeObject<SerializedBurnerData>(json);
                var burnerSlots = ItemSlotFactory.Deserialize(serializedBurnerData.SerializedItems);
                return new BurnerFuelInventory(parent, burnerSlots,serializedBurnerData.InitalDuration, serializedBurnerData.RemainingDuration);;
            }
            catch (JsonSerializationException e)
            {
                Debug.LogWarning(e);
                return null;
            }
        }
        
        public static MachineEnergyInventory DeserializeEnergyMachineInventory(string json, IMachineInstance parent)
        {
            if (json == null)
            {
                return null;
            }
            try
            {
                ulong energy = JsonConvert.DeserializeObject<ulong>(json);
                return new MachineEnergyInventory(energy, parent);
            }
            catch (JsonSerializationException e)
            {
                Debug.LogError(e);
                return null;
            }
            
        }
        
        private class SerializedBurnerData
        {
            public string SerializedItems;
            public uint InitalDuration;
            public uint RemainingDuration;

            public SerializedBurnerData(string serializedItems, uint initalDuration, uint remainingDuration)
            {
                SerializedItems = serializedItems;
                InitalDuration = initalDuration;
                RemainingDuration = remainingDuration;
            }
        }
    }
    
}

