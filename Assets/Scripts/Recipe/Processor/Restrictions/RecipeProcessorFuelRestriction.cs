using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Recipe.Processor.Restrictions
{
    [CreateAssetMenu(fileName = "New Fuel Restriction", menuName = "Crafting/Processor Restriction/Fuel")]
    public class RecipeProcessorFuelRestriction : RecipeProcessorRestrictionObject
    {
        [Header("NOTE: Ensure the item here is a fuel")]
        public List<ItemObject> WhiteListedFuels;
    }
}
