using System;
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
                    return 0.5f;
                case TransmutationEfficency.Half:
                    return 1f;
                default:
                    throw new ArgumentOutOfRangeException(nameof(efficency), efficency, null);
            }
        }
    }
}
