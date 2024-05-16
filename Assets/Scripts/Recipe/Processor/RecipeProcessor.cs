using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Items.Inventory;
using TileEntityModule;

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
    public interface IRegisterableProcessor {
        public GameObject getUIPrefab();
        public InventoryLayout getInventoryLayout();
    }

    public interface IDisplayableProcessor {
        public GameObject getRecipeUI(IRecipe recipe, string processorName);
    }

    public abstract class RecipeProcessor : ScriptableObject, IRecipeProcessor {
        public abstract int getRecipeCount();
        public abstract List<IRecipe> getRecipes();
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
            if (items == null) {
                return false;
            }
            foreach (ItemSlot itemSlot in items) {
                if (itemSlot != null && itemSlot.itemObject != null) {
                    return true;
                }
            }
            return false;
        }

        public override List<IRecipe> getRecipes()
        {
            List<IRecipe> recipes = new List<IRecipe>();
            foreach (IRecipeCollection collection in recipeCollectionList) {
                recipes.AddRange(collection.getRecipes());
            }
            return recipes;
        }

        public Type getCollectionType() {
            return typeof(Collection);
        }
        public void addRecipeCollection(object recipeCollection, int mode) {
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

    public abstract class DisplayableTypedRecipeProcessor<Collection> : TypedRecipeProcessor<Collection>, IRegisterableProcessor, IDisplayableProcessor where Collection : IRecipeCollection
    {
        [SerializeField] protected GameObject uiPrefab;
        [SerializeField] protected InventoryLayout layout;

        public InventoryLayout getInventoryLayout()
        {
            return layout;
        }

        public abstract GameObject getRecipeUI(IRecipe recipe, string processorName);

        public GameObject getUIPrefab()
        {
            return uiPrefab;
        }
    }

}
