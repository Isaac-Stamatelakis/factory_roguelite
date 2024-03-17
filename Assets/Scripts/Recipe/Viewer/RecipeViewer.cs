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
        public void show(Dictionary<RecipeProcessor, List<IRecipe>> recipes) {
            List<RecipeProcessor> processors = recipes.Keys.ToList();
            RecipeProcessorSorter.sortProcessors(processors);
            indicatorController.init(processors,processors[0]);
            recipeProcessorLeftButton.onClick.AddListener(()=> {indicatorController.moveLeft();});   
            recipeProcessorRightButton.onClick.AddListener(()=> {indicatorController.moveRight();}); 
        }
        
    }
}

