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
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TileEntity.Instances.WorkBench {
    public class WorkBenchUI : MonoBehaviour, ITileEntityUI<WorkBenchInstance>, IRecipeProcessorUI
    {
        [SerializeField] private WorkbenchProcessorUI mWorkbenchProcessorUI;
        [FormerlySerializedAs("recipeLookUpList")] [SerializeField] private RecipeLookUpList mRecipeLookUpList;
        [SerializeField] private VerticalLayoutGroup mInventoryUIGroup;
        [SerializeField] private InventoryUI mInventoryUI;
        [SerializeField] private InventoryUI mPlayerInventoryUI;

        public void TryCraftItem()
        {
            PlayerInventory playerInventory = PlayerContainer.getInstance().getInventory();
            if (mRecipeLookUpList.GetCurrentRecipe() is not ItemRecipeObject itemRecipeObject) return;
            var outputs = ItemSlotFactory.FromEditorObjects(itemRecipeObject.Outputs);
            if (!ItemSlotUtils.CanInsertIntoInventory(playerInventory.Inventory, outputs, Global.MaxSize)) return;
            ItemRecipeObjectInstance itemRecipeObjectInstance = new ItemRecipeObjectInstance(itemRecipeObject);
            var itemRecipe = RecipeUtils.TryCraftRecipe<ItemRecipe>(itemRecipeObjectInstance, playerInventory.Inventory, null, RecipeType.Item);
            if (itemRecipe == null) return;
            ItemSlotUtils.InsertInventoryIntoInventory(playerInventory.Inventory,itemRecipe.SolidOutputs, Global.MaxSize);
            playerInventory.Refresh();
        }

        public void DisplayTileEntityInstance(WorkBenchInstance tileEntityInstance)
        {
            mWorkbenchProcessorUI.Initialize(this);
            PlayerInventory playerInventory = PlayerContainer.getInstance().getInventory();
            mPlayerInventoryUI.DisplayInventory(playerInventory.Inventory);
            RecipeProcessorInstance recipeProcessorInstance = RecipeRegistry.GetProcessorInstance(tileEntityInstance.TileEntityObject.WorkBenchRecipeProcessor);
            mRecipeLookUpList.Initialize(recipeProcessorInstance,this, tileEntityInstance.WorkBenchData); // Note this displays the first recipe
            mInventoryUIGroup.gameObject.SetActive(tileEntityInstance.Inventory!=null);
            if (tileEntityInstance.Inventory != null)
            {
                mInventoryUI.DisplayInventory(tileEntityInstance.Inventory);
            }
        }

        public void DisplayRecipe(DisplayableRecipe recipe)
        {
            mWorkbenchProcessorUI.DisplayForCraft(recipe);
        }
    }
}

