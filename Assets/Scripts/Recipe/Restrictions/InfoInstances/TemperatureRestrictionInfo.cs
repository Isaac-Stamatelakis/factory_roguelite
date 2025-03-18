using Recipe.Viewer;

namespace Recipe.Restrictions.InfoInstances
{
    public class TemperatureRestrictionInfo : RecipeRestrictionInfo
    {
        public override string GetRestrictionText(DisplayableRecipe displayableRecipe)
        {
            return "Requires Clean Room";
        }
        
        
   
    }
}
