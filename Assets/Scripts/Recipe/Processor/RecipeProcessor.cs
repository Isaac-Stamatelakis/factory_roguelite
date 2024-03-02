using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    /// <summary>
    /// Processes recipes
    /// </summary>
    
    public interface IRecipeProcessor {

    }

    public abstract class RecipeProcessor : ScriptableObject, IRecipeProcessor {
        public string id;
    }
    public abstract class TypedRecipeProcessor<R> : RecipeProcessor 
    {
        protected Dictionary<int, List<R>> recipesOfMode = new Dictionary<int, List<R>>();
        protected bool itemsNotAllNull(List<ItemSlot> items) {
            foreach (ItemSlot itemSlot in items) {
                if (itemSlot != null && itemSlot.itemObject != null) {
                    return true;
                }
            }
            return false;
        }
    }

}
