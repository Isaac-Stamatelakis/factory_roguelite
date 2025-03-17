using System;
using Recipe.Objects.Restrictions;
using Recipe.Restrictions.InfoInstances;

namespace Recipe.Restrictions
{
    public static class RecipeRestrictionInfoFactory
    {
        public static RecipeRestrictionInfo GetRecipeRestrictionInfo(RecipeRestriction recipeRestriction)
        {
            switch (recipeRestriction)
            {
                case RecipeRestriction.None:
                    return null;
                case RecipeRestriction.Temperature:
                    return new TemperatureRestrictionInfo();
                case RecipeRestriction.CleanRoom:
                    return new CleanRoomRestrictionInfo();
                case RecipeRestriction.Data:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(recipeRestriction), recipeRestriction, null);
            }

            return null;
        }
    }
}
