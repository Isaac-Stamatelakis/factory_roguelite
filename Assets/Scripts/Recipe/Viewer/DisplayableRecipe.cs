using System.Collections.Generic;
using Item.Slot;
using Items;
using Items.Tags;
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
    public abstract class DisplayableRecipe
    {
        public RecipeData RecipeData;
        protected DisplayableRecipe(RecipeData recipeData)
        {
            RecipeData = recipeData;
        }
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
    }
}

