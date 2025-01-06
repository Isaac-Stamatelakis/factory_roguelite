using Recipe.Viewer;
using TileEntity.Instances.Machine.UI;
using TileEntity.Instances.WorkBench;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TileEntity.Instances.Workbench.UI
{
    public class WorkbenchProcessorUI : MonoBehaviour, IRecipeProcessorUI
    {
        [SerializeField] private TileEntityInventoryUI mTileEntityInventoryUI;
        [SerializeField] private Button mCraftButton;
        public void DisplayRecipe(DisplayableRecipe recipe)
        {
            mTileEntityInventoryUI.DisplayRecipe(recipe);
            mCraftButton.gameObject.SetActive(false);
        }

        public void DisplayForCraft(DisplayableRecipe recipe, WorkBenchUI workBenchUIParent)
        {
            mTileEntityInventoryUI.DisplayRecipe(recipe);
            mCraftButton.onClick.AddListener(workBenchUIParent.TryCraftItem);
        }
    }
}
