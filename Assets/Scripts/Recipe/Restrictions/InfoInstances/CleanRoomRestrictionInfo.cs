using Recipe.Viewer;

namespace Recipe.Restrictions.InfoInstances
{
    public class CleanRoomRestrictionInfo : RecipeRestrictionInfo
    {
        public override string GetRestrictionText(DisplayableRecipe displayableRecipe)
        {
            return "Requires Clean Room";
        }
    }
}
