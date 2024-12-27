using UnityEngine;

namespace Recipe.Objects
{
    [CreateAssetMenu(fileName ="New Passive Item Recipe",menuName="Crafting/Recipes/Generator")]
    public class GeneratorItemRecipeObject : PassiveItemRecipeObject
    {
        public ulong EnergyPerTick;
    }
}
