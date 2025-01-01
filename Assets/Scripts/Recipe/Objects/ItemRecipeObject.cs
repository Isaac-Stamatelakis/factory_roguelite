using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Items;
using Recipe.Objects;
using UnityEngine;

namespace Recipe.Objects
{
    [CreateAssetMenu(fileName ="New Item Recipe",menuName="Crafting/Recipes/Item")]
    public class ItemRecipeObject : RecipeObject
    {
        public List<EditorItemSlot> Inputs;
        public List<RandomEditorItemSlot> Outputs;
    }

    public class ItemRecipeObjectInstance
    {
        public ItemRecipeObject ItemRecipeObject;
        public List<ItemSlot> Inputs;
        public ItemRecipeObjectInstance(ItemRecipeObject itemRecipeObject)
        {
            ItemRecipeObject = itemRecipeObject;
            Inputs = ItemSlotFactory.FromEditorObjects(itemRecipeObject.Inputs);
        }
    }

    [System.Serializable]
    public class RandomEditorItemSlot : EditorItemSlot
    {
        [Range(0, 1)] public float Chance = 1f;
    }
}

