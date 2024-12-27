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
        public ulong InputEnergy;
        public ulong EnergyCostPerTick;
        public ItemEnergyRecipe(List<ItemSlot> outputs, ulong inputEnergy, ulong energyCostPerTick) : base(outputs)
        {
            InputEnergy = inputEnergy;
            EnergyCostPerTick = energyCostPerTick;
        }
    }

    public class PassiveItemRecipe : ItemRecipe
    {
        public double Ticks;
        public PassiveItemRecipe(List<ItemSlot> outputs, double ticks) : base(outputs)
        {
            Ticks = ticks;
        }
    }

    public class GeneratorItemRecipe : PassiveItemRecipe
    {
        public ulong EnergyOutputPerTick;
        public GeneratorItemRecipe(List<ItemSlot> outputs, double ticks, ulong energyOutputPerTick) : base(outputs, ticks)
        {
            EnergyOutputPerTick = energyOutputPerTick;
        }
    }

    public static class RecipeSerializationFactory
    {
        public static string Serialize(ItemRecipe itemRecipe, RecipeType recipeType)
        {
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
                        passiveItemRecipe.Ticks
                    );
                    return JsonConvert.SerializeObject(serializedPassiveItemRecipe);
                case RecipeType.Generator:
                    GeneratorItemRecipe generatorItemRecipe = (GeneratorItemRecipe)itemRecipe;
                    SerializedGeneratorItemRecipe serializedGeneratorItem = new SerializedGeneratorItemRecipe(
                        serializedOutputs, 
                        generatorItemRecipe.Ticks,
                        generatorItemRecipe.EnergyOutputPerTick
                    );
                    return JsonConvert.SerializeObject(generatorItemRecipe);
                case RecipeType.EnergyItem:
                    ItemEnergyRecipe itemEnergyRecipe = (ItemEnergyRecipe)itemRecipe;
                    SerializedItemEnergyRecipe serializedItemEnergyRecipe = new SerializedItemEnergyRecipe(
                        serializedOutputs,
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
                        return new PassiveItemRecipe(outputs, serializedPassiveItemRecipe.Ticks) as T;
                    case RecipeType.Generator:
                        SerializedGeneratorItemRecipe serializedGeneratorItemRecipe = (SerializedGeneratorItemRecipe) serializedItemRecipe;
                        return new GeneratorItemRecipe(outputs, serializedGeneratorItemRecipe.Ticks, serializedGeneratorItemRecipe.EnergyOutputPerTick) as T;
                    case RecipeType.EnergyItem:
                        SerializedItemEnergyRecipe serializedItemEnergyRecipe = (SerializedItemEnergyRecipe) serializedItemRecipe;
                        return new ItemEnergyRecipe(outputs, serializedItemEnergyRecipe.Energy, serializedItemEnergyRecipe.EnergyCostPerTick) as T;
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
        public double Ticks;

        public SerializedPassiveItemRecipe(string itemOutputs, double ticks) : base(itemOutputs)
        {
            ItemOutputs = itemOutputs;
            Ticks = ticks;
        }
    }

    public class SerializedGeneratorItemRecipe : SerializedItemRecipe
    {
        public double Ticks;
        public ulong EnergyOutputPerTick;

        public SerializedGeneratorItemRecipe(string itemOutputs, double ticks, ulong energyOutputPerTick) : base(itemOutputs)
        {
            ItemOutputs = itemOutputs;
            Ticks = ticks;
            EnergyOutputPerTick = energyOutputPerTick;
        }
    }

    public class SerializedItemEnergyRecipe : SerializedItemRecipe
    {
        public ulong Energy;
        public ulong EnergyCostPerTick;

        public SerializedItemEnergyRecipe(string itemOutputs, ulong energy, ulong energyCostPerTick) : base(itemOutputs)
        {
            ItemOutputs = itemOutputs;
            Energy = energy;
            EnergyCostPerTick = energyCostPerTick;
        }
    }
}
