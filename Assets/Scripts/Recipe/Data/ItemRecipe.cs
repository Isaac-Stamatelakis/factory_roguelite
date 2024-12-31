using System;
using System.Collections.Generic;
using Items;
using Newtonsoft.Json;
using Recipe.Objects;
using UnityEngine;

namespace Recipe.Data
{
    public class ItemRecipe
    {
        public List<ItemSlot> SolidOutputs;
        public List<ItemSlot> FluidOutputs;
        public ItemRecipe(List<ItemSlot> solidOutputs, List<ItemSlot> fluidOutputs)
        {
            SolidOutputs = solidOutputs;
            FluidOutputs = fluidOutputs;
        }
    }
    public class ItemEnergyRecipe : ItemRecipe
    {
        public ulong InitialCost;
        public ulong InputEnergy;
        public ulong EnergyCostPerTick;
        public ItemEnergyRecipe(List<ItemSlot> solidOutputs, List<ItemSlot> fluidOutputs, ulong initialCost, ulong inputEnergy, ulong energyCostPerTick) : base(solidOutputs, fluidOutputs)
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
        public PassiveItemRecipe(List<ItemSlot> solidOutputs, List<ItemSlot> fluidOutputs, double initalTicks, double remainginTicks) : base(solidOutputs, fluidOutputs)
        {
            InitalTicks = initalTicks;
            RemainingTicks = remainginTicks;
        }
    }

    public class BurnerItemRecipe : PassiveItemRecipe
    {
        public float PassiveSpeed;
        public BurnerItemRecipe(List<ItemSlot> solidOutputs, List<ItemSlot> fluidOutputs, double initalTicks, double remainginTicks, float passiveSpeed) 
            : base(solidOutputs, fluidOutputs, initalTicks, remainginTicks)
        {
            PassiveSpeed = passiveSpeed;
        }
    }
    public class GeneratorItemRecipe : PassiveItemRecipe
    {
        public ulong EnergyOutputPerTick;
        public GeneratorItemRecipe(List<ItemSlot> solidOutputs, List<ItemSlot> fluidOutputs, double initalTicks, double remainginTicks, ulong energyOutputPerTick) : base(solidOutputs,fluidOutputs, initalTicks, remainginTicks)
        {
            EnergyOutputPerTick = energyOutputPerTick;
        }
    }

    public static class RecipeSerializationFactory
    {
        public static string Serialize(ItemRecipe itemRecipe, RecipeType recipeType)
        {
            if (itemRecipe == null) return null;
            string serializedSolidOutputs = ItemSlotFactory.serializeList(itemRecipe.SolidOutputs);
            string serializedFluidOutputs = ItemSlotFactory.serializeList(itemRecipe.FluidOutputs);
            switch (recipeType)
            {
                case RecipeType.Item:
                    SerializedItemRecipe serializedItemRecipe = new SerializedItemRecipe(serializedSolidOutputs, serializedFluidOutputs);
                    return JsonConvert.SerializeObject(serializedItemRecipe);
                case RecipeType.Passive:
                    PassiveItemRecipe passiveItemRecipe = (PassiveItemRecipe)itemRecipe;
                    SerializedPassiveItemRecipe serializedPassiveItemRecipe = new SerializedPassiveItemRecipe(
                        serializedSolidOutputs, 
                        serializedFluidOutputs,
                        passiveItemRecipe.InitalTicks,
                        passiveItemRecipe.RemainingTicks
                    );
                    return JsonConvert.SerializeObject(serializedPassiveItemRecipe);
                case RecipeType.Generator:
                    GeneratorItemRecipe generatorItemRecipe = (GeneratorItemRecipe)itemRecipe;
                    SerializedGeneratorItemRecipe serializedGeneratorItem = new SerializedGeneratorItemRecipe(
                        serializedSolidOutputs, 
                        serializedFluidOutputs,
                        generatorItemRecipe.InitalTicks,
                        generatorItemRecipe.RemainingTicks,
                        generatorItemRecipe.EnergyOutputPerTick
                    );
                    return JsonConvert.SerializeObject(serializedGeneratorItem);
                case RecipeType.Machine:
                    ItemEnergyRecipe itemEnergyRecipe = (ItemEnergyRecipe)itemRecipe;
                    SerializedItemEnergyRecipe serializedItemEnergyRecipe = new SerializedItemEnergyRecipe(
                        serializedSolidOutputs,
                        serializedFluidOutputs,
                        itemEnergyRecipe.InitialCost,
                        itemEnergyRecipe.InputEnergy,
                        itemEnergyRecipe.EnergyCostPerTick
                    );
                    return JsonConvert.SerializeObject(serializedItemEnergyRecipe);
                case RecipeType.Burner:
                    BurnerItemRecipe burnerItemRecipe = (BurnerItemRecipe)itemRecipe;
                    SerializedBurnerItemRecipe serializedBurnerItemRecipe = new SerializedBurnerItemRecipe(
                        serializedSolidOutputs,
                        serializedFluidOutputs,
                        burnerItemRecipe.InitalTicks,
                        burnerItemRecipe.RemainingTicks,
                        burnerItemRecipe.PassiveSpeed
                    );
                    return JsonConvert.SerializeObject(serializedBurnerItemRecipe);
                default:
                    throw new ArgumentOutOfRangeException(nameof(recipeType), recipeType, null);
            }
        }

        public static T Deserialize<T>(string serializedOutputs, RecipeType recipeType) where T : ItemRecipe
        {
            if (serializedOutputs == null) return default;
            SerializedItemRecipe serializedItemRecipe;
            try
            {
                switch (recipeType)
                {
                    case RecipeType.Item:
                        serializedItemRecipe = JsonConvert.DeserializeObject<SerializedItemRecipe>(serializedOutputs);
                        break;
                    case RecipeType.Passive:
                        serializedItemRecipe = JsonConvert.DeserializeObject<SerializedPassiveItemRecipe>(serializedOutputs);
                        break;
                    case RecipeType.Generator:
                        serializedItemRecipe = JsonConvert.DeserializeObject<SerializedGeneratorItemRecipe>(serializedOutputs);
                        break;
                    case RecipeType.Machine:
                        serializedItemRecipe = JsonConvert.DeserializeObject<SerializedItemEnergyRecipe>(serializedOutputs);
                        break;
                    case RecipeType.Burner:
                        serializedItemRecipe = JsonConvert.DeserializeObject<SerializedBurnerItemRecipe>(serializedOutputs);
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
                var solidOutputs = ItemSlotFactory.Deserialize(serializedItemRecipe.ItemOutputs);
                var fluidOutputs = ItemSlotFactory.Deserialize(serializedItemRecipe.FluidOutputs);
                switch (recipeType)
                {
                    case RecipeType.Item:
                        return new ItemRecipe(solidOutputs,fluidOutputs) as T;
                    case RecipeType.Passive:
                        SerializedPassiveItemRecipe serializedPassiveItemRecipe = (SerializedPassiveItemRecipe) serializedItemRecipe;
                        return new PassiveItemRecipe(
                            solidOutputs,
                            fluidOutputs, 
                            serializedPassiveItemRecipe.InitalTicks, 
                            serializedPassiveItemRecipe.RemainingTicks
                        ) as T;
                    case RecipeType.Generator:
                        SerializedGeneratorItemRecipe serializedGeneratorItemRecipe = (SerializedGeneratorItemRecipe) serializedItemRecipe;
                        return new GeneratorItemRecipe(
                            solidOutputs,
                            fluidOutputs, 
                            serializedGeneratorItemRecipe.InitalTicks, 
                            serializedGeneratorItemRecipe.RemainingTicks, 
                            serializedGeneratorItemRecipe.EnergyOutputPerTick
                        ) as T;
                    case RecipeType.Machine:
                        SerializedItemEnergyRecipe serializedItemEnergyRecipe = (SerializedItemEnergyRecipe) serializedItemRecipe;
                        return new ItemEnergyRecipe(
                            solidOutputs,
                            fluidOutputs, 
                            serializedItemEnergyRecipe.InitialEnergy, 
                            serializedItemEnergyRecipe.Energy, 
                            serializedItemEnergyRecipe.EnergyCostPerTick
                        ) as T;
                    case RecipeType.Burner:
                        SerializedBurnerItemRecipe sBurnerRecipe = (SerializedBurnerItemRecipe) serializedItemRecipe;
                        return new BurnerItemRecipe(
                            solidOutputs, 
                            fluidOutputs, 
                            sBurnerRecipe.InitalTicks, 
                            sBurnerRecipe.RemainingTicks, 
                            sBurnerRecipe.PassiveSpeed
                        ) as T;
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
        public string FluidOutputs;

        public SerializedItemRecipe(string itemOutputs, string fluidOutputs)
        {
            ItemOutputs = itemOutputs;
            FluidOutputs = fluidOutputs;
        }
    }
    public class SerializedPassiveItemRecipe : SerializedItemRecipe
    {
        public double RemainingTicks;
        public double InitalTicks;
        public SerializedPassiveItemRecipe(string itemOutputs, string fluidOutputs, double initalTicks, double remainingTicks) : base(itemOutputs, fluidOutputs)
        {
            InitalTicks = initalTicks;
            RemainingTicks = remainingTicks;
        }
    }

    public class SerializedBurnerItemRecipe : SerializedPassiveItemRecipe
    {
        public float PassiveSpeed;
        public SerializedBurnerItemRecipe(string itemOutputs, string fluidOutputs, double initalTicks, double remainingTicks, float passiveSpeed) : base(itemOutputs, fluidOutputs, initalTicks, remainingTicks)
        {
            PassiveSpeed = passiveSpeed;
        }
    }

    public class SerializedGeneratorItemRecipe : SerializedPassiveItemRecipe
    {
        public ulong EnergyOutputPerTick;

        public SerializedGeneratorItemRecipe(string itemOutputs,string fluidOutputs,double initalTicks, double remainingTicks, ulong energyOutputPerTick) : base(itemOutputs, fluidOutputs,initalTicks, remainingTicks)
        {
            EnergyOutputPerTick = energyOutputPerTick;
        }
    }

    public class SerializedItemEnergyRecipe : SerializedItemRecipe
    {
        public ulong InitialEnergy;
        public ulong Energy;
        public ulong EnergyCostPerTick;

        public SerializedItemEnergyRecipe(string itemOutputs,string fluidOutputs, ulong initialEnergy, ulong energy, ulong energyCostPerTick) : base(itemOutputs, fluidOutputs)
        {
            InitialEnergy = initialEnergy;
            Energy = energy;
            EnergyCostPerTick = energyCostPerTick;
        }
    }
}
