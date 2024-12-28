using System;
using System.Collections.Generic;
using Items;
using Newtonsoft.Json;
using UnityEngine;

namespace Recipe.Data
{
    public class ItemRecipe
    {
        public List<ItemSlot> Outputs;
        public ItemRecipe(List<ItemSlot> outputs)
        {
            Outputs = outputs;
        }
    }
    public class ItemEnergyRecipe : ItemRecipe
    {
        public ulong InitialCost;
        public ulong InputEnergy;
        public ulong EnergyCostPerTick;
        public ItemEnergyRecipe(List<ItemSlot> outputs, ulong initialCost, ulong inputEnergy, ulong energyCostPerTick) : base(outputs)
        {
            InitialCost = initialCost;
            InputEnergy = inputEnergy;
            EnergyCostPerTick = energyCostPerTick;
        }
    }

    public class PassiveItemRecipe : ItemRecipe
    {
        public double InitalTicks;
        public double RemainingTicks;
        public PassiveItemRecipe(List<ItemSlot> outputs, double initalTicks, double remainginTicks) : base(outputs)
        {
            InitalTicks = initalTicks;
            RemainingTicks = remainginTicks;
        }
    }

    public class GeneratorItemRecipe : PassiveItemRecipe
    {
        public ulong EnergyOutputPerTick;
        public GeneratorItemRecipe(List<ItemSlot> outputs, double initalTicks, double remainginTicks, ulong energyOutputPerTick) : base(outputs, initalTicks, remainginTicks)
        {
            EnergyOutputPerTick = energyOutputPerTick;
        }
    }

    public static class RecipeSerializationFactory
    {
        public static string Serialize(ItemRecipe itemRecipe, RecipeType recipeType)
        {
            if (itemRecipe == null) return null;
            string serializedOutputs = ItemSlotFactory.serializeList(itemRecipe.Outputs);
            switch (recipeType)
            {
                case RecipeType.Item:
                    SerializedItemRecipe serializedItemRecipe = new SerializedItemRecipe(serializedOutputs);
                    return JsonConvert.SerializeObject(serializedItemRecipe);
                case RecipeType.PassiveItem:
                    PassiveItemRecipe passiveItemRecipe = (PassiveItemRecipe)itemRecipe;
                    SerializedPassiveItemRecipe serializedPassiveItemRecipe = new SerializedPassiveItemRecipe(
                        serializedOutputs, 
                        passiveItemRecipe.InitalTicks,
                        passiveItemRecipe.RemainingTicks
                    );
                    return JsonConvert.SerializeObject(serializedPassiveItemRecipe);
                case RecipeType.Generator:
                    GeneratorItemRecipe generatorItemRecipe = (GeneratorItemRecipe)itemRecipe;
                    SerializedGeneratorItemRecipe serializedGeneratorItem = new SerializedGeneratorItemRecipe(
                        serializedOutputs, 
                        generatorItemRecipe.InitalTicks,
                        generatorItemRecipe.RemainingTicks,
                        generatorItemRecipe.EnergyOutputPerTick
                    );
                    return JsonConvert.SerializeObject(generatorItemRecipe);
                case RecipeType.EnergyItem:
                    ItemEnergyRecipe itemEnergyRecipe = (ItemEnergyRecipe)itemRecipe;
                    SerializedItemEnergyRecipe serializedItemEnergyRecipe = new SerializedItemEnergyRecipe(
                        serializedOutputs,
                        itemEnergyRecipe.InitialCost,
                        itemEnergyRecipe.InputEnergy,
                        itemEnergyRecipe.EnergyCostPerTick
                    );
                    return JsonConvert.SerializeObject(itemEnergyRecipe);
                default:
                    throw new ArgumentOutOfRangeException(nameof(recipeType), recipeType, null);
            }
        }

        public static T Deserialize<T>(string serializedOutputs, RecipeType recipeType) where T : ItemRecipe
        {
            SerializedItemRecipe serializedItemRecipe;
            try
            {
                switch (recipeType)
                {
                    case RecipeType.Item:
                        serializedItemRecipe = JsonConvert.DeserializeObject<SerializedItemRecipe>(serializedOutputs);
                        break;
                    case RecipeType.PassiveItem:
                        serializedItemRecipe = JsonConvert.DeserializeObject<SerializedPassiveItemRecipe>(serializedOutputs);
                        break;
                    case RecipeType.Generator:
                        serializedItemRecipe = JsonConvert.DeserializeObject<SerializedGeneratorItemRecipe>(serializedOutputs);
                        break;
                    case RecipeType.EnergyItem:
                        serializedItemRecipe = JsonConvert.DeserializeObject<SerializedItemEnergyRecipe>(serializedOutputs);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(recipeType), recipeType, null);
                }
            }
            catch (JsonSerializationException e)
            {
                Debug.LogError(e);
                return default;
            }

  
            try
            {
                var outputs = ItemSlotFactory.Deserialize(serializedItemRecipe.ItemOutputs);
                switch (recipeType)
                {
                    case RecipeType.Item:
                        return new ItemRecipe(outputs) as T;
                    case RecipeType.PassiveItem:
                        SerializedPassiveItemRecipe serializedPassiveItemRecipe = (SerializedPassiveItemRecipe) serializedItemRecipe;
                        return new PassiveItemRecipe(outputs, serializedPassiveItemRecipe.InitalTicks, serializedPassiveItemRecipe.RemainingTicks) as T;
                    case RecipeType.Generator:
                        SerializedGeneratorItemRecipe serializedGeneratorItemRecipe = (SerializedGeneratorItemRecipe) serializedItemRecipe;
                        return new GeneratorItemRecipe(outputs, serializedGeneratorItemRecipe.InitalTicks, serializedGeneratorItemRecipe.RemainingTicks, serializedGeneratorItemRecipe.EnergyOutputPerTick) as T;
                    case RecipeType.EnergyItem:
                        SerializedItemEnergyRecipe serializedItemEnergyRecipe = (SerializedItemEnergyRecipe) serializedItemRecipe;
                        return new ItemEnergyRecipe(outputs, serializedItemEnergyRecipe.InitialEnergy, serializedItemEnergyRecipe.Energy, serializedItemEnergyRecipe.EnergyCostPerTick) as T;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(recipeType), recipeType, null);
                }
            }
            catch (JsonSerializationException e)
            {
                Debug.LogError(e);
                return default;
            }
        }
    }

    public class SerializedItemRecipe
    {
        public string ItemOutputs;

        public SerializedItemRecipe(string itemOutputs)
        {
            ItemOutputs = itemOutputs;
        }
    }
    public class SerializedPassiveItemRecipe : SerializedItemRecipe
    {
        public double RemainingTicks;
        public double InitalTicks;
        public SerializedPassiveItemRecipe(string itemOutputs, double initalTicks, double remainingTicks) : base(itemOutputs)
        {
            InitalTicks = initalTicks;
            RemainingTicks = remainingTicks;
        }
    }

    public class SerializedGeneratorItemRecipe : SerializedPassiveItemRecipe
    {
        public double Ticks;
        public ulong EnergyOutputPerTick;

        public SerializedGeneratorItemRecipe(string itemOutputs,double initalTicks, double remainingTicks, ulong energyOutputPerTick) : base(itemOutputs, initalTicks, remainingTicks)
        {
            EnergyOutputPerTick = energyOutputPerTick;
        }
    }

    public class SerializedItemEnergyRecipe : SerializedItemRecipe
    {
        public ulong InitialEnergy;
        public ulong Energy;
        public ulong EnergyCostPerTick;

        public SerializedItemEnergyRecipe(string itemOutputs, ulong initialEnergy, ulong energy, ulong energyCostPerTick) : base(itemOutputs)
        {
            InitialEnergy = initialEnergy;
            Energy = energy;
            EnergyCostPerTick = energyCostPerTick;
        }
    }
}
