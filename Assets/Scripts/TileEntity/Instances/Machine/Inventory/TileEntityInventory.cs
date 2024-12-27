using System;
using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using UnityEngine;
using Items.Inventory;
using Newtonsoft.Json;

namespace TileEntity {
    public class MachineItemInventory : ISolidItemConduitInteractable, IFluidConduitInteractable
    {
        public MachineItemInventory(ITileEntityInstance parent, List<ItemSlot> itemInputs, List<ItemSlot> itemOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOutputs)
        {
            this.parent = parent;
            this.itemInputs = itemInputs;
            this.itemOutputs = itemOutputs;
            this.fluidInputs = fluidInputs;
            this.fluidOutputs = fluidOutputs;
        }

        protected ITileEntityInstance parent;
        public List<ItemSlot> itemInputs;
        public List<ItemSlot> itemOutputs;
        public List<ItemSlot> fluidInputs;
        public List<ItemSlot> fluidOutputs;
        
        public ItemSlot ExtractSolidItem(Vector2Int portPosition)
        {
            return ItemSlotHelper.ExtractFromInventory(itemOutputs);
        }

        public void InsertSolidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            ItemSlotHelper.InsertIntoInventory(itemInputs, itemSlot, Global.MaxSize);
        }

        public ItemSlot ExtractFluidItem(Vector2Int portPosition)
        {
            return ItemSlotHelper.ExtractFromInventory(fluidOutputs);
        }

        public void InsertFluidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            Tier tier = Tier.Basic;
            if (parent.GetTileEntity() is ITieredTileEntity tieredTileEntity)
            {
                tier = tieredTileEntity.GetTier();
            }
            ItemSlotHelper.InsertIntoInventory(fluidInputs, itemSlot, tier.GetFluidStorage());
        }
    }

    public class MachineEnergyInventory : IEnergyConduitInteractable
    {
        public MachineEnergyInventory(ulong energy, ITileEntityInstance parent)
        {
            Energy = energy;
            this.parent = parent;
            Tier tier = Tier.Basic;
            if (parent.GetTileEntity() is ITieredTileEntity tieredTileEntity)
            {
                tier = tieredTileEntity.GetTier();
            }

            maxEnergy = tier.GetEnergyStorage();
        }

        public ulong Energy;
        private ITileEntityInstance parent;
        private ulong maxEnergy;
        public ulong InsertEnergy(ulong amount, Vector2Int portPosition)
        {
            
            if (Energy >= maxEnergy) {
                return 0;
            }
            ulong sum = Energy+=amount;
            if (sum > maxEnergy) {
                Energy = maxEnergy;
                return sum - maxEnergy;
            }
            Energy = sum;
            return amount;
        }

        public ref ulong GetEnergy(Vector2Int portPosition)
        {
            return ref Energy;
        }

        public float GetFillPercent()
        {
            return ((float)Energy)/maxEnergy;
        }
    }

    public static class MachineInventoryFactory
    {
        public static string SerializeItemMachineInventory(MachineItemInventory machineInventory)
        {
            try
            {
                SerializedMachineData serializedMachineData = new SerializedMachineData(
                    ItemSlotFactory.serializeList(machineInventory.itemInputs),
                    ItemSlotFactory.serializeList(machineInventory.itemOutputs),
                    ItemSlotFactory.serializeList(machineInventory.fluidInputs),
                    ItemSlotFactory.serializeList(machineInventory.fluidOutputs)
                );
                return JsonConvert.SerializeObject(serializedMachineData);
            }
            catch (NullReferenceException e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        public static string SerializedEnergyMachineInventory(MachineEnergyInventory machineInventory)
        {
            return JsonConvert.SerializeObject(machineInventory.Energy);
            
            
        }

        public static MachineItemInventory DeserializeMachineInventory(string json, ITileEntityInstance parent)
        {
            try
            {
                SerializedMachineData serializedMachineData =
                    JsonConvert.DeserializeObject<SerializedMachineData>(json);
                var solidInputs = ItemSlotFactory.Deserialize(serializedMachineData.SerializedSolidInputs);
                var solidOutputs = ItemSlotFactory.Deserialize(serializedMachineData.SerializedSolidOutputs);
                var fluidInputs = ItemSlotFactory.Deserialize(serializedMachineData.SerializedFluidInputs);
                var fluidOutputs = ItemSlotFactory.Deserialize(serializedMachineData.SerializedFluidOutputs);
                return new MachineItemInventory(parent,solidInputs, solidOutputs, fluidInputs, fluidOutputs);;
            }
            catch (JsonSerializationException e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        public static MachineEnergyInventory DeserializeEnergyMachineInventory(string json, ITileEntityInstance parent)
        {
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

        private class SerializedMachineData
        {
            public string SerializedSolidInputs;
            public string SerializedSolidOutputs;
            public string SerializedFluidInputs;
            public string SerializedFluidOutputs;

            public SerializedMachineData(string serializedSolidInputs, string serializedSolidOutputs, string serializedFluidInputs, string serializedFluidOutputs)
            {
                SerializedSolidInputs = serializedSolidInputs;
                SerializedSolidOutputs = serializedSolidOutputs;
                SerializedFluidInputs = serializedFluidInputs;
                SerializedFluidOutputs = serializedFluidOutputs;
            }
        }
    }
    
}

