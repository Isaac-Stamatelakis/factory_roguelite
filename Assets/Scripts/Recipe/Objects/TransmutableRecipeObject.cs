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
}
