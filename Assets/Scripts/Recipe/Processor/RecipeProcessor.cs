using System.Collections.Generic;
using Items;
using Recipe.Objects;
using RecipeModule;
using TileEntity.Instances.Machine;
using UnityEngine;

namespace Recipe.Processor {
    [CreateAssetMenu(fileName = "New Recipe Processor", menuName = "Crafting/Processor")]
    public class RecipeProcessor : ScriptableObject
    {
        public GameObject UIPrefab;
        public RecipeType RecipeType;
        public List<RecipeModeCollection> RecipeCollections;
        public List<ModeNameKVP> ModeNamesMap;
        public ItemObject DisplayImage;
        public TileEntityLayoutObject LayoutObject;
        public RecipeProcessorRestrictionObject ProcessorRestrictionObject;

        public void RemoveRecipe(int mode, RecipeObject recipeObject)
        {
            foreach (RecipeModeCollection collection in RecipeCollections)
            {
                if (collection.Mode != mode) continue;
                if (!collection.RecipeCollection.Recipes.Contains(recipeObject)) continue;
                collection.RecipeCollection.Recipes.Remove(recipeObject);
                return;
            }
        }
    }

    

    [System.Serializable]
    public class RecipeModeCollection
    {
        public int Mode;
        public RecipeCollection RecipeCollection;
    }

    [System.Serializable]
    public class ModeNameKVP
    {
        public int Mode;
        public string Name;
    }
}
