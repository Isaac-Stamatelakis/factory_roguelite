using System;
using System.Collections.Generic;
using Items;
using Player;
using Recipe.Objects.Restrictions;
using Recipe.Processor;
using Recipe.Restrictions;
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
        private ProcessorCostRotatorUI processorCostRotatorUI;
        public override void Display(ICatalogueElement element, PlayerGameStageCollection gameStages)
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
            if (displayableRecipe is ItemDisplayableRecipe itemDisplayableRecipe)
            {
                DisplayRecipeCost(itemDisplayableRecipe);
            } else if (displayableRecipe is TransmutationDisplayableRecipe transmutationDisplayableRecipe)
            {
                transmutationDisplayableRecipe.RandomizeStartIndex();
                if (!processorCostRotatorUI)
                {
                    processorCostRotatorUI = gameObject.AddComponent<ProcessorCostRotatorUI>();
                }
                processorCostRotatorUI.Initialize(transmutationDisplayableRecipe,DisplayRecipeCost);
            }
        }

        private void DisplayRecipeCost(ItemDisplayableRecipe itemDisplayableRecipe)
        {
            var costString = RecipeViewerHelper.GetCostStringsFromItemDisplayable(
                itemDisplayableRecipe,
                recipeProcessorDisplayInfo.RecipeProcessorInstance.RecipeProcessorObject.RecipeType
            );
            bool costUICreated = !ReferenceEquals(recipeCostUI, null);
            if ((costString == null || costString.Count == 0))
            {
                if (costUICreated) Destroy(recipeCostUI.gameObject);
                return;
            }
            if (!costUICreated) recipeCostUI = Instantiate(recipeCostUIPrefab, transform);
            List<string> restrictions = GetRestrictionText(itemDisplayableRecipe);
            
            recipeCostUI.Display(costString,restrictions);
        }

        private List<string> GetRestrictionText(DisplayableRecipe displayableRecipe)
        {
            RecipeRestriction recipeRestriction = displayableRecipe.RecipeData.Recipe.RecipeRestriction;
            if (recipeRestriction == RecipeRestriction.None) return new List<string>();
            List<string> restrictions = new List<string>();
           
            RecipeRestrictionInfo recipeRestrictionInfo = RecipeRestrictionInfoFactory.GetRecipeRestrictionInfo(recipeRestriction);
            restrictions.Add(recipeRestrictionInfo.GetRestrictionText(displayableRecipe));
            return restrictions;
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

        public ItemObject GetDisplayItem()
        {
            return RecipeProcessorInstance.RecipeProcessorObject.DisplayImage;
        }

        public string GetPageIndicatorString(int pageIndex)
        {
            return $"Recipe {(pageIndex + 1)}/{Pages.Count}";
        }

        public int GetPageCount()
        {
            return Pages.Count;
        }

        public void DisplayAllElements(PlayerGameStageCollection gameStageCollection)
        {
            List<DisplayableRecipe> recipes = RecipeProcessorInstance.GetAllRecipesToDisplay();
            RecipeProcessorDisplayInfo recipeProcessorDisplayInfo = new RecipeProcessorDisplayInfo(RecipeProcessorInstance, recipes);
            CatalogueElementData catalogueElementData = new CatalogueElementData(recipeProcessorDisplayInfo, CatalogueInfoDisplayType.Recipe);
            CatalogueInfoUtils.DisplayCatalogue(new List<CatalogueElementData>{catalogueElementData},gameStageCollection);
            
        }

        public bool Filter(PlayerGameStageCollection gameStageCollection)
        {
            CatalogueInfoUtils.FilterList(Pages,gameStageCollection);
            return Pages.Count > 0;
        }
    }
    
}
