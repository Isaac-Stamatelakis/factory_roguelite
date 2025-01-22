using System;
using System.Collections.Generic;
using Item.Slot;
using Items;
using Items.Transmutable;
using Recipe.Data;
using Recipe.Objects;
using Recipe.Viewer;
using RecipeModule;
using TileEntity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;

namespace Recipe.Processor
{
    public class ItemRecipeCollection
    {
        private RecipeData[] recipes;
        private Dictionary<string, ushort[]> outputItemRecipes;
        private Dictionary<string, ushort[]> inputItemRecipes;
        private Dictionary<TransmutableItemState, ushort[]> inputStateRecipes;
        private Dictionary<TransmutableItemState, ushort[]> outputStateRecipes;

        public ItemRecipeCollection(RecipeProcessorInstance recipeProcessorInstance)
        {
            var tempRecipes = new List<RecipeData>();
            var tempOutput = new Dictionary<string, List<ushort>>();
            var tempInput = new Dictionary<string, List<ushort>>();
            var tempOutputStateRecipes = new Dictionary<TransmutableItemState, List<ushort>>();
            var tempInputStateRecipes = new Dictionary<TransmutableItemState,List<ushort>>();
            
            
            foreach (RecipeModeCollection recipeModeCollection in recipeProcessorInstance.RecipeProcessorObject.RecipeCollections)
            {
                foreach (RecipeObject recipeObject in recipeModeCollection.RecipeCollection.Recipes)
                {
                    tempRecipes.Add(new RecipeData(recipeModeCollection.Mode,recipeObject,recipeProcessorInstance));
                    ushort recipeIndex = (ushort)(tempRecipes.Count - 1);
                    switch (recipeObject)
                    {
                        case ItemRecipeObject itemRecipeObject:
                            PlaceRecipeIndex(itemRecipeObject.Inputs,tempInput,recipeIndex);
                            PlaceRecipeIndex(itemRecipeObject.Outputs,tempOutput,recipeIndex);
                            break;
                        case TransmutableRecipeObject transmutableRecipeObject:
                            PlaceTransmutationState(transmutableRecipeObject.InputState,tempInputStateRecipes,recipeIndex);
                            PlaceTransmutationState(transmutableRecipeObject.OutputState,tempOutputStateRecipes,recipeIndex);
                            break;
                    }
                }
            }

            recipes = tempRecipes.ToArray();
            outputItemRecipes = ToArrayDict(tempOutput);
            inputItemRecipes = ToArrayDict(tempInput);
            outputStateRecipes = ToArrayDict(tempOutputStateRecipes);
            inputStateRecipes = ToArrayDict(tempInputStateRecipes);
        }

        private void PlaceTransmutationState(TransmutableItemState state, Dictionary<TransmutableItemState, List<ushort>> dict, ushort recipeIndex)
        {
            if (!dict.ContainsKey(state))
            {
                dict[state] = new List<ushort>();
            }
            dict[state].Add(recipeIndex);
        }
        
        private void PlaceRecipeIndex<T>(List<T> slots, Dictionary<string, List<ushort>> dict, ushort recipeIndex) where T : EditorItemSlot
        {
            var includedIds = new HashSet<string>();
            foreach (var slot in slots)
            {
                if (ReferenceEquals(slot?.ItemObject, null)) continue;
                string id = slot.ItemObject.id;
                if (!includedIds.Add(id)) continue;
                if (!dict.ContainsKey(id))
                {
                    dict[id] = new List<ushort>();
                }
                dict[id].Add(recipeIndex);
            }
        }

        private Dictionary<T, ushort[]> ToArrayDict<T>(Dictionary<T, List<ushort>> tempDict)
        {
            Dictionary<T, ushort[]> dict = new Dictionary<T, ushort[]>();
            foreach (var kvp in tempDict)
            {
                dict[kvp.Key] = kvp.Value.ToArray();
            }

            return dict;
        }

        public List<RecipeData> GetInputRecipes(ItemSlot itemSlot)
        {
            return FromDict(itemSlot, inputItemRecipes, inputStateRecipes);
        }

        public List<RecipeData> GetOutputRecipes(ItemSlot itemSlot)
        {
            return FromDict(itemSlot, outputItemRecipes, outputStateRecipes);
        }

        private List<RecipeData> FromDict(ItemSlot itemSlot, Dictionary<string, ushort[]> itemDict,  Dictionary<TransmutableItemState, ushort[]> transmutationDict)
        {
            if (ReferenceEquals(itemSlot?.itemObject,null)) return null;
            
            var validRecipes = new List<RecipeData>();
            if (itemDict.TryGetValue(itemSlot.itemObject.id, out var itemIndices))
            {
                foreach (ushort index in itemIndices)
                {
                    validRecipes.Add(recipes[index]);
                }
            }
            
            if (itemSlot.itemObject is TransmutableItemObject transmutableItemObject 
                && transmutationDict.TryGetValue(transmutableItemObject.getState(), out var stateRecipeIndices)
                )
            {
                foreach (ushort index in stateRecipeIndices)
                {
                    validRecipes.Add(recipes[index]);
                }
                
            }
            return validRecipes;
        }

    }
    public class RecipeProcessorInstance
    {
        private RecipeProcessor recipeProcessorObject;
        public RecipeProcessor RecipeProcessorObject => recipeProcessorObject;
        private Dictionary<int, Dictionary<ulong, ItemRecipeObjectInstance[]>> modeRecipeDict;
        private Dictionary<int, Dictionary<TransmutableItemState, TransmutableRecipeObject>> modeRecipeTransmutation;
        private Dictionary<int, string> modeNameDict;
        private ItemRecipeCollection collection;
        
        
        public RecipeProcessorInstance(RecipeProcessor recipeProcessorObject)
        {
            this.recipeProcessorObject = recipeProcessorObject;
            InitializeModeRecipeDict();
            collection = new ItemRecipeCollection(this);
            modeNameDict = new Dictionary<int, string>();
            foreach (var modeNameKVP in recipeProcessorObject.ModeNamesMap)
            {
                modeNameDict[modeNameKVP.Mode] = modeNameKVP.Name;
            }
        }
        
        private void InitializeModeRecipeDict()
        {
            modeRecipeDict = new Dictionary<int, Dictionary<ulong, ItemRecipeObjectInstance[]>>();
            modeRecipeTransmutation = new Dictionary<int, Dictionary<TransmutableItemState, TransmutableRecipeObject>>();
            int hashCollisions = 0;
            var tempModeRecipeDict = new Dictionary<int, Dictionary<ulong, List<ItemRecipeObject>>>();
            foreach (RecipeModeCollection recipeModeCollection in recipeProcessorObject.RecipeCollections)
            {
                int mode = recipeModeCollection.Mode;
                if (!tempModeRecipeDict.ContainsKey(mode))
                {
                    tempModeRecipeDict[mode] = new Dictionary<ulong, List<ItemRecipeObject>>();
                }

                if (!modeRecipeTransmutation.ContainsKey(mode))
                {
                    modeRecipeTransmutation[mode] = new Dictionary<TransmutableItemState, TransmutableRecipeObject>();
                }
                List<ItemRecipeObject> itemRecipes = new List<ItemRecipeObject>();
                foreach (RecipeObject recipeObject in recipeModeCollection.RecipeCollection.Recipes)
                {
                    RecipeUtils.InsertValidRecipes(recipeProcessorObject, recipeObject, itemRecipes,modeRecipeTransmutation[mode]);
                }
                
                foreach (ItemRecipeObject itemRecipeObject in itemRecipes)
                {
                    var inputs = new List<ItemSlot>();
                    foreach (EditorItemSlot editorItemSlot in itemRecipeObject.Inputs)
                    {
                        ItemSlot itemSlot = ItemSlotFactory.FromEditorObject(editorItemSlot);
                        if (!ReferenceEquals(itemSlot?.itemObject, null)) inputs.Add(itemSlot);
                    }
                    if (inputs.Count == 0) continue;
                    
                    ulong hash = RecipeUtils.HashItemInputs(inputs);
                    if (tempModeRecipeDict[mode].ContainsKey(hash))
                    {
                        hashCollisions++;
                    }
                    else
                    {
                        tempModeRecipeDict[mode][hash] = new List<ItemRecipeObject>();
                    }
                    tempModeRecipeDict[mode][hash].Add(itemRecipeObject);
                }
            }

            foreach (var kvp in tempModeRecipeDict)
            {
                if (kvp.Value.Count == 0) continue;
                int mode = kvp.Key;
                if (!modeRecipeDict.ContainsKey(mode))
                {
                    modeRecipeDict[mode] = new Dictionary<ulong, ItemRecipeObjectInstance[]>();
                }
                
                foreach (var (hash, list) in kvp.Value)
                {
                    var recipeObjectInstances = new ItemRecipeObjectInstance[list.Count];
                    for (int i = 0; i < list.Count; i++)
                    {
                        recipeObjectInstances[i] = new ItemRecipeObjectInstance(list[i]);
                    }
                    modeRecipeDict[mode][hash] = recipeObjectInstances;
                }
            }
    
            List<int> emptyModes = new List<int>();
            foreach (var kvp in modeRecipeTransmutation)
            {
                if (kvp.Value.Count == 0) emptyModes.Add(kvp.Key);
            }
            foreach (int mode in emptyModes)
            {
                modeRecipeTransmutation.Remove(mode);
            }

            if (hashCollisions > 0) Debug.LogWarning($"RecipeProcessor '{RecipeProcessorObject.name} item recipe dict has {hashCollisions} hash collisions");
        }
        
        public T GetRecipe<T>(int mode, List<ItemSlot> solidItems, List<ItemSlot> fluidItems) where T : ItemRecipe
        {
            bool canTransmute = modeRecipeTransmutation.ContainsKey(mode);
            if (canTransmute)
            {
                var transmutableItems = new List<ItemSlot>();
                foreach (ItemSlot itemSlot in solidItems)
                {
                    if (ItemSlotUtils.IsItemSlotNull(itemSlot) || itemSlot.itemObject is not TransmutableItemObject)
                    {
                        continue;
                    }
                    transmutableItems.Add(itemSlot);
                }
                foreach (ItemSlot itemSlot in fluidItems)
                {
                    if (ItemSlotUtils.IsItemSlotNull(itemSlot) || itemSlot.itemObject is not TransmutableItemObject)
                    {
                        continue;
                    }
                    transmutableItems.Add(itemSlot);
                }
                Dictionary<TransmutableItemState, TransmutableRecipeObject> stateRecipeDict = modeRecipeTransmutation[mode];
                foreach (ItemSlot transmutableItem in transmutableItems)
                {
                    var transItemObject = transmutableItem.itemObject as TransmutableItemObject;
                    var state = transItemObject!.getState();
                    if (!stateRecipeDict.TryGetValue(state, out var transmutableRecipe)) continue;
                    var result = RecipeUtils.TryCraftTransmutableRecipe<T>(transmutableRecipe,transmutableItem,transItemObject.getMaterial(),recipeProcessorObject.RecipeType);
                    if (result != null) return result;
                }
            }
            if (!modeRecipeDict.TryGetValue(mode, out var recipeDict))
            {
                return default;
            }
            ulong hash = RecipeUtils.HashItemInputs(solidItems,fluidItems);
            if (!recipeDict.TryGetValue(hash, out var recipeObjects))
            {
                return default;
            }

            return RecipeUtils.TryCraftRecipe<T>(recipeObjects, solidItems, fluidItems, recipeProcessorObject.RecipeType);
            

        }
        
        public List<DisplayableRecipe> GetRecipesForItem(ItemSlot itemSlot)
        {
            List<DisplayableRecipe> displayableRecipes = new List<DisplayableRecipe>();
            
            foreach (RecipeData recipeData in collection.GetOutputRecipes(itemSlot))
            {
                displayableRecipes.Add(RecipeFactory.ToDisplayableRecipe(recipeData, itemSlot));
            }

            return displayableRecipes;
        }
        
        public List<DisplayableRecipe> GetRecipesWithItem(ItemSlot itemSlot)
        {
            List<DisplayableRecipe> displayableRecipes = new List<DisplayableRecipe>();
            
            foreach (RecipeData recipeData in collection.GetInputRecipes(itemSlot))
            {
                displayableRecipes.Add(RecipeFactory.ToDisplayableRecipe(recipeData, itemSlot));
            }

            return displayableRecipes;
        }
        
        public List<DisplayableRecipe> GetAllRecipesToDisplay()
        {
            List<DisplayableRecipe> recipes = new List<DisplayableRecipe>();
            foreach (var kvp in modeRecipeDict)
            {
                int mode = kvp.Key;
                foreach (var recipeKvp in kvp.Value)
                {
                    foreach (ItemRecipeObjectInstance recipeInstance in recipeKvp.Value)
                    {
                        var outputs = ItemSlotFactory.EditToChanceSlots(recipeInstance.ItemRecipeObject.Outputs);
                        ItemSlotUtils.sortInventoryByState(recipeInstance.Inputs,out var solidInputs, out var fluidInputs);
                        ItemSlotUtils.sortInventoryByState(outputs,out var solidOutputs, out var fluidOutputs);
                        RecipeData recipeData = new RecipeData(mode, recipeInstance.ItemRecipeObject, this);
                        recipes.Add(new ItemDisplayableRecipe(recipeData, solidInputs,solidOutputs,fluidInputs,fluidOutputs));
                    }
                }
            }

            foreach (var kvp in modeRecipeTransmutation)
            {
                int mode = kvp.Key;
                foreach (var (inputState, transRecipe) in kvp.Value)
                {
                    List<TransmutableItemMaterial> materials = ItemRegistry.GetInstance().GetAllMaterials();
                    List<ItemSlot> inputs = new List<ItemSlot>();
                    List<ItemSlot> outputs = new List<ItemSlot>();
                    foreach (TransmutableItemMaterial material in materials)
                    {
                        bool inputMatch = false;
                        bool outputMatch = false;
                        foreach (TransmutableStateOptions stateOptions in material.MaterialOptions.States)
                        {
                            
                            var state = stateOptions.state;
                            if (state == transRecipe.InputState)
                            {
                                inputMatch = true;
                            }

                            if (state == transRecipe.OutputState)
                            {
                                outputMatch = true;
                            }

                            if (!inputMatch || !outputMatch) continue;
                            var (input,output) = TransmutableItemUtils.Transmute(material,transRecipe.InputState,transRecipe.OutputState);
                            inputs.Add(input);
                            outputs.Add(output);
                            break;
                        }
                    }
                    if (inputs.Count == 0 || outputs.Count == 0) continue;
                    RecipeData recipeData = new RecipeData(mode, transRecipe, this);
                    
                    recipes.Add(new TransmutationDisplayableRecipe(recipeData, inputs,outputs,inputState.getMatterState(),transRecipe.OutputState.getMatterState()));
                }
            }

            return recipes;
        }

        public Dictionary<int, List<DisplayableRecipe>> GetRecipesToDisplayByMode()
        {
            var displayableRecipes = GetAllRecipesToDisplay();
            var displayableRecipesByMode = new Dictionary<int, List<DisplayableRecipe>>();
            foreach (DisplayableRecipe displayableRecipe in displayableRecipes)
            {
                int mode = displayableRecipe.RecipeData.Mode;
                if (!displayableRecipesByMode.ContainsKey(mode))
                {
                    displayableRecipesByMode[mode] = new List<DisplayableRecipe>();
                }
                displayableRecipesByMode[mode].Add(displayableRecipe);
            }

            return displayableRecipesByMode;
        }

        public bool HasModeName(int mode)
        {
            return modeNameDict.ContainsKey(mode);
        }
        public string GetModeName(int mode)
        {
            if (!modeNameDict.TryGetValue(mode, value: out var value))
            {
                value = mode.ToString();
            }

            return value;
        }

        public int GetCount()
        {
            int count = 0;
            foreach (var kvp in modeRecipeDict)
            {
                count += kvp.Value.Count;
            }

            foreach (var kvp in modeRecipeTransmutation)
            {
                count += kvp.Value.Count;
            }

            return count;
        }
        
    }

    public static class RecipeFactory
    {
        public static ItemRecipe CreateRecipe(RecipeType recipeType, ItemRecipeObject recipeObject)
        {
            List<ItemSlot> outputCopy = ItemSlotFactory.FromRandomEditorObjects(recipeObject.Outputs);
            ItemSlotUtils.sortInventoryByState(outputCopy,out var solidOutputs, out var fluidOutputs);
            switch (recipeType)
            {
                case RecipeType.Item:
                    return new ItemRecipe(solidOutputs, fluidOutputs);
                case RecipeType.Passive:
                    PassiveItemRecipeObject passiveItemRecipeObject = (PassiveItemRecipeObject)recipeObject;
                    return new PassiveItemRecipe(solidOutputs,fluidOutputs, passiveItemRecipeObject.Ticks, passiveItemRecipeObject.Ticks);
                case RecipeType.Generator:
                    GeneratorItemRecipeObject generatorRecipeObject = (GeneratorItemRecipeObject)recipeObject;
                    return new GeneratorItemRecipe(solidOutputs,fluidOutputs, generatorRecipeObject.Ticks, generatorRecipeObject.Ticks, generatorRecipeObject.EnergyPerTick);
                case RecipeType.Machine:
                    ItemEnergyRecipeObject itemRecipeObject = (ItemEnergyRecipeObject)recipeObject;
                    return new ItemEnergyRecipe(solidOutputs,fluidOutputs, itemRecipeObject.TotalInputEnergy, itemRecipeObject.TotalInputEnergy, itemRecipeObject.MinimumEnergyPerTick);
                case RecipeType.Burner:
                    BurnerRecipeObject burnerRecipeObject = (BurnerRecipeObject)recipeObject;
                    return new BurnerItemRecipe(solidOutputs, fluidOutputs, burnerRecipeObject.Ticks, burnerRecipeObject.Ticks, burnerRecipeObject.PassiveSpeed);
                default:
                    throw new ArgumentOutOfRangeException(nameof(recipeType), recipeType, null);
            }
        }
        
        public static ItemRecipe GetTransmutationRecipe(RecipeType recipeType, TransmutableItemMaterial material, TransmutableItemState outputState, ItemSlot output)
        {
            List<ItemSlot> solid = null;
            List<ItemSlot> fluid = null;
            ItemState itemState = outputState.getMatterState();
            switch (itemState)
            {
                case ItemState.Solid:
                    solid = new List<ItemSlot> { output };
                    break;
                case ItemState.Fluid:
                    fluid = new List<ItemSlot> { output };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (recipeType)
            {
                case RecipeType.Item:
                    return new ItemRecipe(solid, fluid);
                case RecipeType.Machine:
                    ulong usage = material.tier.GetMaxEnergyUsage();
                    ulong cost = 32 * usage; // TODO change this
                    return new ItemEnergyRecipe(solid,fluid, cost, cost,usage);
                default:
                    throw new ArgumentOutOfRangeException(nameof(recipeType), recipeType, null);
            }
        }

        public static ItemDisplayableRecipe ToDisplayableRecipe(RecipeData recipeData, ItemSlot itemSlot)
        {
            
            switch (recipeData.Recipe)
            {
                case ItemRecipeObject itemRecipeObject:
                    return ToDisplayableRecipe(recipeData, itemRecipeObject);
                case TransmutableRecipeObject transmutableRecipeObject:
                    if (itemSlot.itemObject is not TransmutableItemObject transmutableItemObject)
                    {
                        Debug.LogWarning("Tried to get transmutable item displayable recipe for non transmutable item");
                        return null;
                    }
                    return ToDisplayableRecipe(recipeData, transmutableRecipeObject,transmutableItemObject);
                default:
                    throw new ArgumentOutOfRangeException(nameof(recipeData.Recipe), recipeData.Recipe, null);
            }
        }

        private static ItemDisplayableRecipe ToDisplayableRecipe(RecipeData recipeData, ItemRecipeObject itemRecipeObject)
        {
            var inputs = ItemSlotFactory.FromEditorObjects(itemRecipeObject.Inputs);
            var outputs = ItemSlotFactory.EditToChanceSlots(itemRecipeObject.Outputs);
            ItemSlotUtils.sortInventoryByState(inputs,out var solidInputs, out var fluidInputs);
            ItemSlotUtils.sortInventoryByState(outputs,out var solidOutputs, out var fluidOutputs);
            return new ItemDisplayableRecipe(recipeData, solidInputs, solidOutputs, fluidInputs, fluidOutputs);
        }

        private static ItemDisplayableRecipe ToDisplayableRecipe(RecipeData recipeData, TransmutableRecipeObject transmutableRecipeObject, TransmutableItemObject inputItem)
        {
            var inputMatterState = transmutableRecipeObject.InputState.getMatterState();
            var outputMatterState = transmutableRecipeObject.OutputState.getMatterState();
            List<ItemSlot> solidInput = null;
            List<ChanceItemSlot> solidOutput = null;
            List<ItemSlot> fluidInput = null;
            List<ChanceItemSlot> fluidOutput = null;
            var (input, output) = TransmutableItemUtils.Transmute(inputItem.getMaterial(), transmutableRecipeObject.InputState, transmutableRecipeObject.OutputState);
            var chanceOutput = ItemSlotFactory.ToChanceSlot(output);
            switch (inputMatterState)
            {
                case ItemState.Solid:
                    solidInput = new List<ItemSlot> { input };
                    break;
                case ItemState.Fluid:
                    fluidInput = new List<ItemSlot> { input };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            switch (outputMatterState)
            {
                case ItemState.Solid:
                    solidOutput = new List<ChanceItemSlot> {  chanceOutput };
                    break;
                case ItemState.Fluid:
                    fluidOutput = new List<ChanceItemSlot> { chanceOutput };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return new ItemDisplayableRecipe(recipeData, solidInput, solidOutput, fluidInput, fluidOutput);
        }
    }
}
