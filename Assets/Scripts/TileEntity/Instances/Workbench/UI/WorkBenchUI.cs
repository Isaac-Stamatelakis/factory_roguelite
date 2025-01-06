using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Items.Inventory;
using UnityEngine;
using RecipeModule;
using PlayerModule;
using Recipe;
using Recipe.Data;
using Recipe.Objects;
using Recipe.Processor;
using Recipe.Viewer;
using TileEntity.Instances.Workbench;
using TileEntity.Instances.Workbench.UI;
using TileEntity.Instances.WorkBenchs;
using UnityEngine.UI;

namespace TileEntity.Instances.WorkBench {
    public class WorkBenchUI : MonoBehaviour, ITileEntityUI<WorkBenchInstance>, IRecipeProcessorUI
    {
        [SerializeField] private WorkbenchProcessorUI mWorkbenchProcessorUI;
        [SerializeField] private RecipeLookUpList recipeLookUpList;
        [SerializeField] private InventoryUI mPlayerInventoryUI;
        private RecipeProcessor recipeProcessor;

        public void TryCraftItem()
        {
            PlayerInventory playerInventory = PlayerContainer.getInstance().getInventory();
            var itemRecipeObject = recipeLookUpList.GetCurrentRecipe() as ItemRecipeObject;
            ItemRecipeObjectInstance itemRecipeObjectInstance = new ItemRecipeObjectInstance(itemRecipeObject);
            var itemRecipe = RecipeUtils.TryCraftRecipe<ItemRecipe>(itemRecipeObjectInstance, playerInventory.Inventory, null, RecipeType.Item);
            if (itemRecipe == null) return;
            ItemSlotUtils.InsertInventoryIntoInventory(playerInventory.Inventory,itemRecipe.SolidOutputs, Global.MaxSize);
            playerInventory.Refresh();
        }

        public void DisplayTileEntityInstance(WorkBenchInstance tileEntityInstance)
        {
            mWorkbenchProcessorUI.Initialize(this);
            recipeProcessor = tileEntityInstance.TileEntityObject.WorkBenchRecipeProcessor;
            PlayerInventory playerInventory = PlayerContainer.getInstance().getInventory();
            mPlayerInventoryUI.DisplayInventory(playerInventory.Inventory);
            RecipeProcessorInstance recipeProcessorInstance = RecipeRegistry.GetProcessorInstance(tileEntityInstance.TileEntityObject.WorkBenchRecipeProcessor);
            recipeLookUpList.Initialize(recipeProcessorInstance,this, tileEntityInstance.WorkBenchData); // Note this displays the first recipe
        }

        public void DisplayRecipe(DisplayableRecipe recipe)
        {
            mWorkbenchProcessorUI.DisplayForCraft((ItemDisplayableRecipe)recipe);
        }
    }
}

