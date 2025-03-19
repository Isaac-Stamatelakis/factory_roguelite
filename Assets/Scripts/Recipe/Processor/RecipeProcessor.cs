using System.Collections.Generic;
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
        public Sprite Sprite;
        public TileEntityLayoutObject LayoutObject;
        public RecipeProcessorRestrictionObject ProcessorRestrictionObject;
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
