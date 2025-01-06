using System.Collections.Generic;
using Recipe;
using Recipe.Data;
using Recipe.Objects;
using Recipe.Processor;
using Recipe.Viewer;
using TileEntity.Instances.WorkBench;
using TileEntity.Instances.Workbench.UI.RecipeList;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace TileEntity.Instances.Workbench.UI
{
    public class RecipeLookUpList : MonoBehaviour
    {
        [SerializeField] private VerticalLayoutGroup mContentList;
        [SerializeField] private RecipeListHeader headerPrefab;
        [SerializeField] private RecipeListElement listElementPrefab;
        [SerializeField] private Color highLightColor;
        private Dictionary<int, List<DisplayableRecipe>> modeRecipes;
        private Dictionary<int, (RecipeListHeader, List<RecipeListElement>)> modeElementDict;
        private int currentMode = -1;
        private int currentIndex = -1;
        private Color defaultElementColor;
        private  IRecipeProcessorUI recipeProcessorUI;
        public void Initialize(RecipeProcessorInstance recipeProcessor, IRecipeProcessorUI recipeProcessorUI)
        {
            GlobalHelper.deleteAllChildren(mContentList.transform);
            modeRecipes = recipeProcessor.GetRecipesToDisplayByMode();
            modeElementDict = new Dictionary<int, (RecipeListHeader, List<RecipeListElement>)>();
            foreach (var (mode, recipes) in modeRecipes)
            {
                List<RecipeListElement> elements = new List<RecipeListElement>();
                string modeName = recipeProcessor.GetModeName(mode);
                RecipeListHeader header = Instantiate(headerPrefab, mContentList.transform);
                header.Display(this,modeName,mode);
                for (int i = 0; i < recipes.Count; i++)
                {
                    RecipeListElement recipeListElement = Instantiate(listElementPrefab, mContentList.transform);
                    recipeListElement.Display(recipes[i],this,mode,i);
                    elements.Add(recipeListElement);
                }
                modeElementDict[mode] = (header,elements);
            }

            defaultElementColor = listElementPrefab.GetComponent<Image>().color;
            this.recipeProcessorUI = recipeProcessorUI;
            Select(0,0);
        }

        public RecipeObject GetCurrentRecipe()
        {
            return modeRecipes[currentMode][currentIndex].RecipeData.Recipe;
        }
        public void Select(int mode, int index)
        {
            if (mode == currentMode && index == currentIndex) return;
            var (header, recipeElements) = modeElementDict[mode];
            recipeElements[index].SetColor(highLightColor);
            if (currentIndex >= 0 && currentMode >= 0)
            {
                modeElementDict[currentMode].Item2[currentIndex].SetColor(defaultElementColor);
            }
            currentIndex = index;
            currentMode = mode;
            recipeProcessorUI.DisplayRecipe(modeRecipes[mode][index]);
        }

        public void ToggleHeader(int mode)
        {
            var (header, recipeElements) = modeElementDict[mode];
            if (recipeElements.Count == 0) return;
            bool toggleState = !recipeElements[0].gameObject.activeInHierarchy;
            foreach (RecipeListElement recipeElement in recipeElements)
            {
                recipeElement.gameObject.SetActive(toggleState);
            }
        }
    }
}
