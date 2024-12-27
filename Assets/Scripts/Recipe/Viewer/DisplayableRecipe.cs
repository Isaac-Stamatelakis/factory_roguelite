using Recipe.Data;
using Recipe.Objects;
using Recipe.Processor;

namespace Recipe.Viewer
{
    public struct DisplayableRecipe
    {
        public int Mode;
        public RecipeObject Recipe;
        public RecipeProcessorInstance ProcessorInstance;
        public DisplayableRecipe(int mode, RecipeObject recipe, RecipeProcessorInstance processorInstance)
        {
            Mode = mode;
            Recipe = recipe;
            ProcessorInstance = processorInstance;
        }
    }
}

