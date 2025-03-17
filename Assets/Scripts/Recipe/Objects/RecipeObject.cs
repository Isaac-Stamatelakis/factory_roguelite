using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Recipe.Objects.Restrictions;

namespace Recipe.Objects {
    public abstract class RecipeObject : ScriptableObject
    {
        public RecipeRestriction RecipeRestriction = RecipeRestriction.None;
    }   
    
}