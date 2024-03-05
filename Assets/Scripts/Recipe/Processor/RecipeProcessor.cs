using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RecipeModule {
    /// <summary>
    /// Processes recipes
    /// </summary>
    
    public interface IRecipeProcessor {
        public int getRecipeCount();
    }
    public interface IInitableRecipeProcessor {
        public void init();
    }

    public abstract class RecipeProcessor : ScriptableObject, IRecipeProcessor {
        public abstract int getRecipeCount();
    }

    public interface ITypedRecipeProcessor {
        public Type getCollectionType();
        public void addRecipeCollection(object recipeCollection, int mode);
        public void resetRecipeCollection();
        public int getModeCount();
        public int getRecipeCount();
        public void init();
    }
    public abstract class TypedRecipeProcessor<Collection> : RecipeProcessor, ITypedRecipeProcessor, IInitableRecipeProcessor where Collection : IRecipeCollection
    {
        // Unity cannot seralize dictionaries
        [SerializeField] public List<Collection> recipeCollectionList;
        protected Dictionary<int, Collection> recipesOfMode;
        public void init() {
            recipesOfMode = new Dictionary<int, Collection>();
            foreach (Collection collection in recipeCollectionList) {
                int mode = collection.getMode();
                if (recipesOfMode.ContainsKey(mode)) {
                    Debug.Log(name + " has duplicate modes for " + mode);
                    continue;
                }
                recipesOfMode[mode] = collection;
            }
        }
        protected bool itemsNotAllNull(List<ItemSlot> items) {
            foreach (ItemSlot itemSlot in items) {
                if (itemSlot != null && itemSlot.itemObject != null) {
                    return true;
                }
            }
            return false;
        }

        public Type getCollectionType() {
            return typeof(Collection);
        }
        public void addRecipeCollection(object recipeCollection, int mode) {
            /*
            if (recipesOfMode.ContainsKey(mode)) {
                Debug.LogError(name + " already contains mode " + mode);
                return;
            }
            */
            if (recipeCollection is not Collection casted) {
                Debug.LogError(name + " tried to set invalid recipe collection type");
                return;
            }
            recipeCollectionList.Add(casted);
        }

        public void resetRecipeCollection()
        {
            this.recipeCollectionList = new List<Collection>();
        }

        public int getModeCount()
        {
            return recipeCollectionList.Count;
        }

        public override int getRecipeCount()
        {
            int count = 0;
            foreach (Collection collection in recipeCollectionList) {
                count += collection.getRecipeCount();
            }
            return count;
        }
    }

}
