using System;
using Item.Transmutation;
using Recipe.Data;
using TileEntity;
using UnityEngine;

namespace Recipe.Objects
{
    [CreateAssetMenu(fileName = "New Transmutable Recipe", menuName = "Crafting/Recipes/Transmutable")]
    
    public class TransmutableRecipeObject : RecipeObject
    {
        public TransmutableItemState InputState;
        public TransmutableItemState OutputState;
        public TransmutationEfficency Efficency;
        
        public Tier MinimumTier = Tier.Untiered;
        public Tier MaximumTier = Tier.Disabled;

        public bool CanCraftTier(Tier tier)
        {
            return tier >= MinimumTier && tier <= MaximumTier;
        }
    }
    public enum TransmutationEfficency
    {
        Max = 0,
        Half = 1,
        Third = 2,
        Quarter = 3,
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
                case TransmutationEfficency.Third:
                    return 0.33f;
                case TransmutationEfficency.Quarter:
                    return 0.25f;
                default:
                    throw new ArgumentOutOfRangeException(nameof(efficency), efficency, null);
            }
        }
    }
}
