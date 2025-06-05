using System.Collections.Generic;
using Item.Burnables;
using Item.GameStage;
using Item.Slot;
using Items;
using Items.Tags;
using Items.Transmutable;
using Player;
using Recipe.Data;
using Recipe.Objects;
using Recipe.Processor;
using TileEntity;
using UnityEngine;

namespace Recipe.Viewer
{
    public class RecipeData
    {
        public int Mode;
        public RecipeObject Recipe;
        public RecipeProcessorInstance ProcessorInstance;

        public RecipeData(int mode, RecipeObject recipe, RecipeProcessorInstance processorInstance)
        {
            Mode = mode;
            Recipe = recipe;
            ProcessorInstance = processorInstance;
        }
    }
    public abstract class DisplayableRecipe : IGameStageItemDisplay
    {
        public RecipeData RecipeData;
        protected DisplayableRecipe(RecipeData recipeData)
        {
            RecipeData = recipeData;
        }

        public abstract bool FilterStage(PlayerGameStageCollection gameStageCollection);
    }

    public class ChanceItemSlot : ItemSlot
    {
        public float chance;
        public ChanceItemSlot(ItemObject itemObject, uint amount, ItemTagCollection tags, float chance) : base(itemObject, amount, tags)
        {
            this.chance = chance;
        }
    }
    public class ItemDisplayableRecipe : DisplayableRecipe
    {
        public Tier Tier;
        public List<ItemSlot> SolidInputs;
        public List<ChanceItemSlot> SolidOutputs;
        public List<ItemSlot> FluidInputs;
        public List<ChanceItemSlot> FluidOutputs;

        public ItemDisplayableRecipe(RecipeData recipeData, List<ItemSlot> solidInputs, List<ChanceItemSlot> solidOutputs, List<ItemSlot> fluidInputs, List<ChanceItemSlot> fluidOutputs) : base(recipeData)
        {
            SolidInputs = solidInputs;
            SolidOutputs = solidOutputs;
            FluidInputs = fluidInputs;
            FluidOutputs = fluidOutputs;
            Tier = GetHighestTierItem();
        }

        public ItemDisplayableRecipe(RecipeData recipeData, List<ItemSlot> solidInputs, List<ChanceItemSlot> solidOutputs, List<ItemSlot> fluidInputs, List<ChanceItemSlot> fluidOutputs, Tier tier): base(recipeData)
        {
            SolidInputs = solidInputs;
            SolidOutputs = solidOutputs;
            FluidInputs = fluidInputs;
            FluidOutputs = fluidOutputs;
            Tier = tier;
        }

        private Tier GetHighestTierItem()
        {
            // Tier of a recipe is given by the highest tier of its output items
            Tier highest = Tier.Untiered;
            IterateItems(SolidOutputs);
            IterateItems(FluidOutputs);
            return highest;

            void IterateItems(List<ChanceItemSlot> chanceItemSlots)
            {
                if (FluidOutputs == null) return;
                foreach (ChanceItemSlot chanceItemSlot in chanceItemSlots)
                {
                    Tier tier = chanceItemSlot?.itemObject?.GetTier() ?? Tier.Untiered;
                    if (tier > highest) highest = tier;
                }
            }
        }
        public override bool FilterStage(PlayerGameStageCollection gameStageCollection)
        {
            int tierInt = (int)Tier;
            return gameStageCollection.HasStage(tierInt.ToString());
        }
    }
    
    public class TransmutationDisplayableRecipe : DisplayableRecipe
    {
        public List<ItemSlot> Inputs;
        public List<ItemSlot> Outputs;
        public ItemState InputState;
        public ItemState OutputState;
        public int InitialDisplayIndex;

        public TransmutationDisplayableRecipe(RecipeData recipeData, List<ItemSlot> inputs, List<ItemSlot> outputs, ItemState inputState, ItemState outputState) : base(recipeData)
        {
            Inputs = inputs;
            Outputs = outputs;
            InputState = inputState;
            OutputState = outputState;
        }

        public override bool FilterStage(PlayerGameStageCollection gameStageCollection)
        {
            return true;
        }

        public void RandomizeStartIndex()
        {
            
        }

        public ItemDisplayableRecipe ToItemDisplayableRecipe(int index)
        {
            if (index < 0 || index >= Inputs.Count) return null;
            List<ItemSlot> inputListWrapper = new List<ItemSlot> { Inputs[index] };
            ItemSlot output = Outputs[index];
            Tier tier = output?.itemObject?.GetTier() ?? Tier.Untiered;
            ChanceItemSlot chanceItemSlot = new ChanceItemSlot(output?.itemObject, output?.amount ?? 0, output?.tags, 1);
            List<ChanceItemSlot> outputListWrapper = new List<ChanceItemSlot> { chanceItemSlot };
            // TODO fluid and tile items
            return new ItemDisplayableRecipe(RecipeData,inputListWrapper,outputListWrapper,null,null,tier);
        }
    }
}

