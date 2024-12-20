using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using Items;

namespace RecipeModule.Viewer {
    public static class RecipeViewerHelper
    {
        private static string path = "Assets/UI/Recipe/RecipeViewer.prefab";
        public static void displayUsesOfItem(ItemSlot itemSlot) {
            RecipeRegistry recipeRegistry = RecipeRegistry.getInstance();
            Dictionary<RecipeProcessor, List<IRecipe>> recipesWithItemInInput = recipeRegistry.getRecipesWithItemInInput(itemSlot);
            // If is processor, show recipes it makes
            if (itemSlot.itemObject is TileItem tileItem && tileItem.tileEntity is IProcessorTileEntity tileEntityProcessor) {
                RecipeProcessor processor = tileEntityProcessor.getRecipeProcessor();
                recipesWithItemInInput[processor] = recipeRegistry.getRecipeProcessorRecipes(processor);
            }
            if (recipesWithItemInInput.Count == 0) {
                return;
            }
            GlobalUIController globalUIController = GlobalUIContainer.getInstance().getUiController();
            RecipeViewer viewer = getViewer();
            if (viewer == null) {
                return;
            }
            viewer.show(recipesWithItemInInput);
            globalUIController.setGUI(viewer.gameObject);
        }
        public static void displayCraftingOfItem(ItemSlot itemSlot) {
            
            Dictionary<RecipeProcessor, List<IRecipe>> recipesWithItemInOutput = RecipeRegistry.getInstance().getRecipesWithItemInOutput(itemSlot);
            if (recipesWithItemInOutput.Count == 0) {
                return;
            }
            GlobalUIController globalUIController = GlobalUIContainer.getInstance().getUiController();
            RecipeViewer viewer = getViewer();
            if (viewer == null) {
                return;
            }
            viewer.show(recipesWithItemInOutput);
            globalUIController.setGUI(viewer.gameObject);
        }

        public static void displayUsesOfProcessor(RecipeProcessor processor) {
            List<IRecipe> recipes = RecipeRegistry.getInstance().getRecipeProcessorRecipes(processor);
            Dictionary<RecipeProcessor, List<IRecipe>> recipesOfProcessor = new Dictionary<RecipeProcessor, List<IRecipe>>();
            recipesOfProcessor[processor] = recipes;
            RecipeViewer viewer = getViewer();
            if (viewer == null) {
                return;
            }
            GlobalUIController globalUIController = GlobalUIContainer.getInstance().getUiController();
            viewer.show(recipesOfProcessor);
            globalUIController.setGUI(viewer.gameObject);
        }

        private static RecipeViewer getViewer() {
            return AddressableLoader.getPrefabComponentInstantly<RecipeViewer>(path);
        }
    }
}

