using System;
using Item.Transmutation;
using Recipe.Data;
using UnityEngine;

namespace Recipe.Objects
{
    [CreateAssetMenu(fileName = "New Transmutable Recipe", menuName = "Crafting/Recipes/Transmutable")]
    
    public class TransmutableRecipeObject : RecipeObject
    {
        public TransmutableItemState InputState;
        public TransmutableItemState OutputState;
        public TransmutationEfficency Efficency;
    }
    public enum TransmutationEfficency
    {
        Max,
        Half
    }

    public static class TransmutableExtension
    {
        public static float Value(this TransmutationEfficency efficency)
        {
            switch (efficency)
            {
                case TransmutationEfficency.Max:
                    return 1f;
                case TransmutationEfficency.Half:
                    return 0.5f;
                default:
                    throw new ArgumentOutOfRangeException(nameof(efficency), efficency, null);
            }
        }
    }
}
