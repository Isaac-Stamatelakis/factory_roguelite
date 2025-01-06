using System.Collections.Generic;

namespace Recipe.Viewer
{
    public interface IRecipeProcessorUI
    {
        public void DisplayRecipe(DisplayableRecipe recipe);
    }
}
