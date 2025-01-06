using System.Collections.Generic;
using Recipe;
using Recipe.Data;
using Recipe.Processor;
using Recipe.Viewer;
using TileEntity.Instances.Workbench.UI.RecipeList;
using UnityEngine;
using UnityEngine.UI;

namespace TileEntity.Instances.Workbench.UI
{
    public class RecipeLookUpList : MonoBehaviour
    {
        [SerializeField] private VerticalLayoutGroup mContentList;
        [SerializeField] private RecipeListHeader headerPrefab;
        [SerializeField] private RecipeListElement listElementPrefab;
        private Dictionary<int, List<DisplayableRecipe>> modeRecipes;
        public void Initialize(RecipeProcessorInstance recipeProcessor)
        {
            modeRecipes = recipeProcessor.GetRecipesToDisplayByMode();
            foreach (var (mode, recipes) in modeRecipes)
            {
                string modeName = recipeProcessor.GetModeName(mode);
                RecipeListHeader header = Instantiate(headerPrefab, mContentList.transform);
                header.Display(this,modeName,mode);
                for (int i = 0; i < recipes.Count; i++)
                {
                    RecipeListElement recipeListElement = Instantiate(listElementPrefab, mContentList.transform);
                    recipeListElement.Display(recipes[i],this,mode,i);
                }
            }
        }

        public void Select(int mode, int index)
        {
            Debug.Log($"MODE: {mode} INDEX: {index}");
        }
    }
}
