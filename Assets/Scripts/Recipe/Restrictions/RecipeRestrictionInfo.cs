using Recipe.Viewer;

namespace Recipe.Restrictions
{
    public abstract class RecipeRestrictionInfo
    {
        public abstract string GetRestrictionText(DisplayableRecipe displayableRecipe);
    }
}
