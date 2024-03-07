using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    public interface IRecipeCollection {
        public void setRecipes(Recipe[] recipes);
        public void setMode(int mode);
        public int getMode();
        public int getRecipeCount();
    }
    public abstract class RecipeCollection<T> : ScriptableObject, IRecipeCollection where T : Recipe
    {
        [SerializeField] public List<T> recipes;
        [SerializeField] public int mode;

        public int getMode()
        {
            return mode;
        }
        public void setMode(int mode) {
            this.mode = mode;
        }

        public int getRecipeCount()
        {
            return recipes.Count;
        }

        public void setRecipes(Recipe[] recipes)
        {
            this.recipes = new List<T>();
            foreach (Recipe recipe in recipes) {
                if (recipe is not T valid) {
                    Debug.LogError("Invalid Recipe type in " + name);
                    continue;
                }
                this.recipes.Add(valid);
            }
        }
    }
}

