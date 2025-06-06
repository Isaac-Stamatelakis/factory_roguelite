using UnityEngine;

namespace Recipe.Objects
{
    [CreateAssetMenu(fileName ="New Passive Item Recipe",menuName="Crafting/Recipes/Passive")]
    public class PassiveItemRecipeObject : ItemRecipeObject
    {
        public float Seconds;
    }
}
