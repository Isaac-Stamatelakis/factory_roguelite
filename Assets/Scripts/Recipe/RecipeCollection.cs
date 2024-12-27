using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Recipe.Objects;

namespace Recipe {
    [CreateAssetMenu(fileName = "RecipeCollection", menuName = "Crafting/RecipeCollection")]
    public class RecipeCollection : ScriptableObject
    {
        public List<RecipeObject> Recipes;
    }
}

