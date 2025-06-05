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
        }

        public override bool FilterStage(PlayerGameStageCollection gameStageCollection)
        {
            if (SolidOutputs != null)
            {
                foreach (ChanceItemSlot chanceItemSlot in SolidOutputs)
                {
                    if (gameStageCollection.HasStage(chanceItemSlot?.itemObject?.GetGameStageObject())) return true;
                }
            }
            
            if (FluidOutputs != null)
            {
                foreach (ChanceItemSlot chanceItemSlot in FluidOutputs)
                {
                    if (gameStageCollection.HasStage(chanceItemSlot?.itemObject?.GetGameStageObject())) return true;
                }
            }
            

            return false;
        }
    }
    
    public class TransmutationDisplayableRecipe : DisplayableRecipe
    {
        public List<ItemSlot> Inputs;
        public List<ItemSlot> Outputs;
        public ItemState InputState;
        public ItemState OutputState;

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

        public ItemDisplayableRecipe ToItemDisplayableRecipe(int index)
        {
            if (index < 0 || index >= Inputs.Count) return null;
            List<ItemSlot> inputListWrapper = new List<ItemSlot> { Inputs[index] };
            ItemSlot output = Outputs[index];
            ChanceItemSlot chanceItemSlot = new ChanceItemSlot(output?.itemObject, output?.amount ?? 0, output?.tags, 1);
            List<ChanceItemSlot> outputListWrapper = new List<ChanceItemSlot> { chanceItemSlot };
            // TODO fluid and tile items
            return new ItemDisplayableRecipe(RecipeData,inputListWrapper,outputListWrapper,null,null);
        }
    }
}

