using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GUIModule;
using System.Linq;

namespace RecipeModule.Viewer {
    public class RecipeViewer : MonoBehaviour
    {
        [SerializeField] public TextMeshProUGUI recipeProcessorTitle;
        [SerializeField] public TextMeshProUGUI pageIndicator;
        [SerializeField] public Button recipeProcessorLeftButton;
        [SerializeField] public Button recipeProcessorRightButton;
        [SerializeField] public Button recipeLeftButton;
        [SerializeField] public Button recipeRightButton;
        [SerializeField] public Transform processorContainer;
        [SerializeField] public RecipeProcessorIndicatorController indicatorController;
        private List<RecipeProcessor> orderedProcessors;
        private int currentProcessorIndex;
        private int currentRecipeIndex;
        private Dictionary<RecipeProcessor, List<IRecipe>> processorRecipes;
        public void show(Dictionary<RecipeProcessor, List<IRecipe>> processorRecipes) {
            this.processorRecipes = processorRecipes;
            orderedProcessors = processorRecipes.Keys.ToList();
            RecipeProcessorSorter.sortProcessors(orderedProcessors);
            RecipeProcessor processorToShow = orderedProcessors[0];
            indicatorController.init(orderedProcessors,processorToShow);
            recipeProcessorLeftButton.onClick.AddListener(moveRecipeProcessorLeft);   
            recipeProcessorRightButton.onClick.AddListener(moveRecipeProcessorRight); 
            recipeLeftButton.onClick.AddListener(moveRecipeLeft);
            recipeRightButton.onClick.AddListener(moveRecipeRight);
            // ints are initalized at 0, so displays processor 0 with recipe 0
            display();

        }

        public void OnDestroy() {
            recipeProcessorLeftButton.onClick.RemoveAllListeners();
            recipeProcessorRightButton.onClick.RemoveAllListeners();
            recipeLeftButton.onClick.RemoveAllListeners();
            recipeRightButton.onClick.RemoveAllListeners();
        }

        private void moveRecipeProcessorLeft() {
            indicatorController.moveLeft();
            currentProcessorIndex = Mathf.Abs((currentProcessorIndex-1) % orderedProcessors.Count);
            display();
        }

        private void moveRecipeProcessorRight() {
            indicatorController.moveRight();
            currentProcessorIndex = Mathf.Abs((currentProcessorIndex+1) % orderedProcessors.Count);
            display();
        }

        private void moveRecipeLeft() {
            RecipeProcessor processor = orderedProcessors[currentProcessorIndex];
            List<IRecipe> recipes = processorRecipes[processor];
            currentRecipeIndex = currentRecipeIndex-1;
            if (currentRecipeIndex < 0) {
                currentRecipeIndex = recipes.Count-1;
            }
            display();
        }

        private void moveRecipeRight() {
            RecipeProcessor processor = orderedProcessors[currentProcessorIndex];
            List<IRecipe> recipes = processorRecipes[processor];
            currentRecipeIndex = Mathf.Abs((currentRecipeIndex+1) % recipes.Count);
            display();
        }

        private void display() {
            RecipeProcessor processor = orderedProcessors[currentProcessorIndex];
            recipeProcessorTitle.text = processor.name;
            if (processor is not IDisplayableProcessor displayableProcessor) {
                return;
            } 
            List<IRecipe> recipes = processorRecipes[processor];
            if (recipes.Count < currentRecipeIndex) {
                Debug.LogError("Tried to display recipe with out of range index");
                return;
            }
            setPageIndicatorText(currentRecipeIndex,recipes.Count);
            IRecipe recipe = recipes[currentRecipeIndex];
            GameObject processorUI = displayableProcessor.getRecipeUI(recipe,processor.name);
            processorUI.transform.SetParent(processorContainer,false);
        }

        private void setPageIndicatorText(int index, int count) {
            string text = (index+1) + "/" + count;
            pageIndicator.text = text;
        }
        
    }
}

