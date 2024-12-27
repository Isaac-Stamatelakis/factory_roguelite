using System.Collections;
using System.Collections.Generic;
using Items;
using Recipe.Objects;
using UnityEngine;

namespace Recipe.Objects
{
    [CreateAssetMenu(fileName ="New Item Recipe",menuName="Crafting/Recipes/Item")]
    public class ItemRecipeObject : RecipeObject
    {
        public List<EditorItemSlot> Inputs;
        public List<EditorItemSlot> Outputs;
    }
}

