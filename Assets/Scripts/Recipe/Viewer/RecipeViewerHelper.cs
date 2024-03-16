using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GUIModule;
using TileEntityModule;

namespace RecipeModule.Viewer {
    public static class RecipeViewerHelper
    {
        private static string path = "UI/RecipeViewer";
        public static void displayUsesOfItem(ItemObject itemObject) {
            
            GlobalUIController globalUIController = GlobalUIContainer.getInstance().getUiController();
            RecipeViewer viewer = getViewer();
            if (viewer == null) {
                return;
            }
            RecipeRegistry recipeRegistry = RecipeRegistry.getInstance();
            Dictionary<RecipeProcessor, List<Recipe>> recipesWithItemInInput = recipeRegistry.getRecipesWithItemInInput(itemObject);
            // If is processor, show recipes it makes
            if (itemObject is TileItem tileItem && tileItem.tileEntity is IProcessor tileEntityProcessor) {
                foreach (RecipeProcessor processor in tileEntityProcessor.getProcessors()) {
                    recipesWithItemInInput[processor] = recipeRegistry.getRecipeProcessorRecipes(processor);
                }
            }
            if (recipesWithItemInInput.Count == 0) {
                return;
            }
            viewer.show(recipesWithItemInInput);
            globalUIController.setGUI(viewer.gameObject);
        }
        public static void displayCraftingOfItem(ItemObject itemObject) {
            GlobalUIController globalUIController = GlobalUIContainer.getInstance().getUiController();
            RecipeViewer viewer = getViewer();
            if (viewer == null) {
                return;
            }
            Dictionary<RecipeProcessor, List<Recipe>> recipesWithItemInOutput = RecipeRegistry.getInstance().getRecipesWithItemInOutput(itemObject);
            if (recipesWithItemInOutput.Count == 0) {
                return;
            }
            viewer.show(recipesWithItemInOutput);
            globalUIController.setGUI(viewer.gameObject);
        }

        private static RecipeViewer getViewer() {
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null) {
                Debug.LogError("RecipeViewer prefab is null");
                return null;
            }
            GameObject instantiated = GameObject.Instantiate(prefab);
            RecipeViewer recipeViewer = instantiated.GetComponent<RecipeViewer>();
            if (recipeViewer == null) {
                Debug.LogError("RecipeViewer prefab doesn't have RecipeViewer component");
                return null;
            }
            return recipeViewer;
        }
    }
}

