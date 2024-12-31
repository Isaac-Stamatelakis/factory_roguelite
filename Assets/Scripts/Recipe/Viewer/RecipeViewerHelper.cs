using System.Collections.Generic;
using Item.Slot;
using Recipe.Processor;
using TileEntity.Instances.WorkBenchs;
using UI;

namespace Recipe.Viewer {
    public static class RecipeViewerHelper
    {
        public static void DisplayUsesOfItem(ItemSlot itemSlot) {
            RecipeRegistry recipeRegistry = RecipeRegistry.GetInstance();
            var recipesWithItemInInput = recipeRegistry.GetRecipesWithItemInInput(itemSlot);
            // If is processor, show recipes it makes
            if (itemSlot.itemObject is TileItem tileItem && tileItem.tileEntity is IProcessorTileEntity tileEntityProcessor) {
                RecipeProcessor processor = tileEntityProcessor.GetRecipeProcessor();
                recipesWithItemInInput[processor] = recipeRegistry.GetRecipeProcessorRecipes(processor);
            }
            if (recipesWithItemInInput.Count == 0) {
                return;
            }
            RecipeViewer viewer = MainCanvasController.TInstance.DisplayUIElement<RecipeViewer>(MainSceneUIElement.RecipeViewer);
            viewer.show(recipesWithItemInInput);
            CanvasController.Instance.DisplayObject(viewer.gameObject);
        }
        public static void DisplayCraftingOfItem(ItemSlot itemSlot) {
            var recipesWithItemInOutput = RecipeRegistry.GetInstance().GetRecipesWithItemInOutput(itemSlot);
            if (recipesWithItemInOutput.Count == 0) {
                return;
            }

            RecipeViewer viewer = MainCanvasController.TInstance.DisplayUIElement<RecipeViewer>(MainSceneUIElement.RecipeViewer);
            viewer.show(recipesWithItemInOutput);
            CanvasController.Instance.DisplayObject(viewer.gameObject);
        }

        public static void DisplayUsesOfProcessor(RecipeProcessor processor) {
            List<DisplayableRecipe> recipes = RecipeRegistry.GetInstance().GetRecipeProcessorRecipes(processor);
            var recipesOfProcessor = new Dictionary<RecipeProcessor, List<DisplayableRecipe>>
                {
                    [processor] = recipes
                };
            RecipeViewer viewer = MainCanvasController.TInstance.DisplayUIElement<RecipeViewer>(MainSceneUIElement.RecipeViewer);
            viewer.show(recipesOfProcessor);
            CanvasController.Instance.DisplayObject(viewer.gameObject);
        }
    }
}

