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
        [SerializeField] private TextMeshProUGUI mTitleText;
        [SerializeField] private TileEntityInventoryUI mTileEntityInventoryUI;
        [SerializeField] private Button mCraftButton;
        public void DisplayRecipe(DisplayableRecipe recipe)
        {
            mTileEntityInventoryUI.DisplayRecipe(recipe);
            mCraftButton.gameObject.SetActive(false);
        }

        public void DisplayForCraft(ItemDisplayableRecipe recipe)
        {
            mTileEntityInventoryUI.DisplayRecipe(recipe);
            mTitleText.text = recipe.SolidOutputs[0].itemObject.name;
        }

        public void Initialize(WorkBenchUI workBenchUI)
        {
            mCraftButton.onClick.AddListener(workBenchUI.TryCraftItem);
        }
    }
}
