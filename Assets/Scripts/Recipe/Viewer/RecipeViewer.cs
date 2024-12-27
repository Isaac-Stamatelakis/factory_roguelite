using System.Collections.Generic;
using System.Linq;
using Recipe.Objects;
using Recipe.Processor;
using RecipeModule.Viewer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Recipe.Viewer {
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
        private Dictionary<RecipeProcessor, List<DisplayableRecipe>> processorRecipes;
        private GameObject currentUI;
        public Dictionary<RecipeProcessor, List<DisplayableRecipe>> ProcessorRecipes { get => processorRecipes; set => processorRecipes = value; }

        public void show(Dictionary<RecipeProcessor, List<DisplayableRecipe>> processorRecipes) {
            this.processorRecipes = processorRecipes;
            orderedProcessors = OrderProcessors(this.processorRecipes);
            //RecipeProcessorSorter.sortProcessors(orderedProcessors);
            RecipeProcessor processorToShow = orderedProcessors[0];
            indicatorController.Initialize(this, orderedProcessors,processorToShow);
            recipeProcessorLeftButton.onClick.AddListener(() => MoveByAmount(-1));   
            recipeProcessorRightButton.onClick.AddListener(() => MoveByAmount(1)); 
            recipeLeftButton.onClick.AddListener(MoveRecipeLeft);
            recipeRightButton.onClick.AddListener(MoveRecipeRight);
            // ints are initalized at 0, so displays processor 0 with recipe 0
            Display();

        }

        public void OnDestroy() {
            recipeProcessorLeftButton.onClick.RemoveAllListeners();
            recipeProcessorRightButton.onClick.RemoveAllListeners();
            recipeLeftButton.onClick.RemoveAllListeners();
            recipeRightButton.onClick.RemoveAllListeners();
        }
        public void MoveByAmount(int amount) {
            currentRecipeIndex = 0;
            currentProcessorIndex = Global.modInt(currentProcessorIndex+amount,orderedProcessors.Count);
            indicatorController.MoveByAmount(amount);
            Display();
        }

        public void DisplayUsesOfProcessor(int offset) {
            currentProcessorIndex = Global.modInt(currentProcessorIndex+offset,orderedProcessors.Count);
            RecipeProcessor processor = orderedProcessors[currentProcessorIndex];
            RecipeViewerHelper.DisplayUsesOfProcessor(processor);
        }

        private List<RecipeProcessor> OrderProcessors(Dictionary<RecipeProcessor, List<DisplayableRecipe>> processorRecipes)
        {
            if (processorRecipes == null || processorRecipes.Count == 0)
            {
                return new List<RecipeProcessor>();
            }

            return processorRecipes.Keys
                .OrderBy(processor => processor.name)
                .ToList();
        }

        private void MoveRecipeLeft() {
            RecipeProcessor processor = orderedProcessors[currentProcessorIndex];
            List<DisplayableRecipe> recipes = processorRecipes[processor];
            // modulus gives negatives
            currentRecipeIndex = currentRecipeIndex-1;
            if (currentRecipeIndex < 0) {
                currentRecipeIndex = recipes.Count-1;
            }
            Display();
        }

        private void MoveRecipeRight() {
            RecipeProcessor processor = orderedProcessors[currentProcessorIndex];
            List<DisplayableRecipe> recipes = processorRecipes[processor];
            currentRecipeIndex = Mathf.Abs((currentRecipeIndex+1) % recipes.Count);
            Display();
        }

        private void SetProcessorUI(RecipeProcessorInstance recipeProcessorInstance)
        {
            if (currentUI != null)
            {
                Destroy(currentUI);
            }
            currentUI = Instantiate(recipeProcessorInstance.RecipeProcessorObject.UIPrefab, transform, false);
        }
        private void Display() {
            RecipeProcessor processor = orderedProcessors[currentProcessorIndex];
            recipeProcessorTitle.text = processor.name;
            
            List<DisplayableRecipe> recipes = processorRecipes[processor];
            if (recipes.Count < currentRecipeIndex) {
                Debug.LogError("Tried to display recipe with out of range index");
                return;
            }
            SetPageIndicatorText(currentRecipeIndex,recipes.Count);
            DisplayableRecipe recipe = recipes[currentRecipeIndex];
            currentUI.GetComponent<IRecipeProcessorUI>().DisplayRecipe(recipe);
        }

        private void SetPageIndicatorText(int index, int count) {
            string text = (index+1) + "/" + count;
            pageIndicator.text = text;
        }
        
    }
}

