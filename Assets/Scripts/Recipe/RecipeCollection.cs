using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    public class RecipeCollection : ScriptableObject
    {
        [SerializeField] public Recipe[] recipes;
    }
}

