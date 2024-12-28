using System.Collections.Generic;
using RecipeModule;
using TileEntity.Instances.Machine;
using UnityEngine;

namespace Recipe.Processor {
    [CreateAssetMenu(fileName = "New Recipe Processor", menuName = "Crafting/Processor/Standard")]
    public class RecipeProcessor : ScriptableObject
    {
        public GameObject UIPrefab;
        public RecipeType RecipeType;
        public List<RecipeModeCollection> RecipeCollections;
    }

    

    [System.Serializable]
    public class RecipeModeCollection
    {
        public int Mode;
        public RecipeCollection RecipeCollection;
    }
}
