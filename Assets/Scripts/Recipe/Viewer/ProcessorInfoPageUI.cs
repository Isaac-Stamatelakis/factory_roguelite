using System.Collections.Generic;
using Recipe.Processor;
using UI.Catalogue.InfoViewer;
using Unity.VisualScripting;
using UnityEngine;

namespace Recipe.Viewer
{
    public class ProcessorInfoPageUI : CatalogueInfoUI
    {
        [SerializeField] private RecipeRequirementUI recipeCostUIPrefab;
        private RecipeProcessorDisplayInfo recipeProcessorDisplayInfo;
        private IRecipeProcessorUI recipeProcessorUI;
        private RecipeRequirementUI recipeCostUI;
        
        public override void Display(ICatalogueElement element)
        {
            recipeProcessorDisplayInfo = (RecipeProcessorDisplayInfo)element;
            GameObject recipePrefab = recipeProcessorDisplayInfo.RecipeProcessorInstance.RecipeProcessorObject.UIPrefab;
            GameObject recipeUI = Instantiate(recipePrefab,transform);
            recipeProcessorUI = recipeUI.GetComponent<IRecipeProcessorUI>();
            DisplayPage(0);
           
        }

        public override void DisplayPage(int pageIndex)
        {
            DisplayableRecipe displayableRecipe = recipeProcessorDisplayInfo.Pages[pageIndex];
            recipeProcessorUI.DisplayRecipe(displayableRecipe);
            DisplayRecipeCost(displayableRecipe);
        }

        private void DisplayRecipeCost(DisplayableRecipe displayableRecipe)
        {
            var costString = RecipeViewerHelper.GetRecipeCostStrings(
                displayableRecipe,
                recipeProcessorDisplayInfo.RecipeProcessorInstance.RecipeProcessorObject.RecipeType
            );
            bool costUICreated = !ReferenceEquals(recipeCostUI, null);
            if ((costString == null || costString.Count == 0))
            {
                if (costUICreated) Destroy(recipeCostUI.gameObject);
                return;
            }
            if (!costUICreated) recipeCostUI = Instantiate(recipeCostUIPrefab, transform);
            
            
            recipeCostUI.Display(costString);
        }
    }

    public class RecipeProcessorDisplayInfo : ICatalogueElement
    {
        public readonly RecipeProcessorInstance RecipeProcessorInstance;
        public readonly List<DisplayableRecipe> Pages;

        public RecipeProcessorDisplayInfo(RecipeProcessorInstance recipeProcessorInstance, List<DisplayableRecipe> pages)
        {
            this.RecipeProcessorInstance = recipeProcessorInstance;
            this.Pages = pages;
        }

        public string GetName()
        {
            return RecipeProcessorInstance.RecipeProcessorObject.name;
        }

        public Sprite GetSprite()
        {
            return RecipeProcessorInstance.RecipeProcessorObject.Sprite;
        }

        public string GetPageIndicatorString(int pageIndex)
        {
            return $"Recipe {(pageIndex + 1)}/{Pages.Count}";
        }

        public int GetPageCount()
        {
            return Pages.Count;
        }

        public void DisplayAllElements()
        {
            List<DisplayableRecipe> recipes = RecipeProcessorInstance.GetAllRecipesToDisplay();
            RecipeProcessorDisplayInfo recipeProcessorDisplayInfo = new RecipeProcessorDisplayInfo(RecipeProcessorInstance, recipes);
            CatalogueElementData catalogueElementData = new CatalogueElementData(recipeProcessorDisplayInfo, CatalogueInfoDisplayType.Recipe);
            CatalogueInfoUtils.DisplayCatalogue(new List<CatalogueElementData>{catalogueElementData});
            
        }
    }
    
}
