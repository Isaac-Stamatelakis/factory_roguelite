using System.Collections.Generic;
using Items;
using Items.Transmutable;
using UnityEngine;

namespace Recipe.Objects.Generation
{
    [CreateAssetMenu(fileName = "New Recipe Generator",menuName="Crafting/Recipe Generator")]
    public class RecipeGenerator : ScriptableObject
    {
        public RecipeCollection RecipeCollection;
        public RecipeObject Template;
        public RecipeType RecipeType;
        public int Multiplier = 1;
        public List<int> InputAmounts;
        public List<int> OutputAmounts;
        public List<RecipeGenerationInputList> Inputs;
        public List<RecipeItemObjectList> Outputs;

        public List<ItemRecipeObject> GeneratedRecipes;


    }
    [System.Serializable]
    public class RecipeGenerationInputList
    {
        public List<RecipeGenerationInput> Inputs;
    }
    
    [System.Serializable]
    public class RecipeItemObjectList
    {
        public List<ItemObject> Inputs;
    }
    
    [System.Serializable]
    public class RecipeGenerationInput
    {
        public RecipeGenerationInputMode Mode;
        public ItemObject ItemObject;
        public TransmutableItemMaterial Material;
        public TransmutableItemState ItemState;
    }

    public enum RecipeGenerationInputMode
    {
        Object = 0,
        Material = 1
    }
}
