using UnityEngine;

namespace Recipe.Objects.Restrictions
{
    public enum RecipeRestriction
    {
        None = 0,
        Temperature = 1,
        CleanRoom = 2,
        Data = 3
    }

    public enum BooleanRecipeRestriction
    {
        CleanRoom = 1,
        GreenHouse = 2,
    }
}
