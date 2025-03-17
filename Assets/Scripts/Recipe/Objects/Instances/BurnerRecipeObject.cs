using UnityEngine;

namespace Recipe.Objects
{
    [CreateAssetMenu(fileName = "Burner Recipe",menuName="Crafting/Recipes/Burner")]
    public class BurnerRecipeObject : PassiveItemRecipeObject
    {
        [Range(0, 1)] public float PassiveSpeed = 0f;
    }
}