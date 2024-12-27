using RecipeModule;
using UnityEngine;

namespace Recipe.Objects
{
    [CreateAssetMenu(fileName ="New Item Energy Recipe",menuName="Crafting/Recipes/Item Energy")]
    public class ItemEnergyRecipeObject : ItemRecipeObject
    {
        public ulong TotalInputEnergy;
        public ulong MinimumEnergyPerTick;
    }
}
