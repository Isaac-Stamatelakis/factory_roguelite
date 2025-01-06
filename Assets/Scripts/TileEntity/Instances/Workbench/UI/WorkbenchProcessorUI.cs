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

        public void DisplayForCraft(DisplayableRecipe recipe)
        {
            mTileEntityInventoryUI.DisplayRecipe(recipe);
            mTitleText.text = recipe switch
            {
                ItemDisplayableRecipe itemDisplayableRecipe => itemDisplayableRecipe.SolidOutputs[0].itemObject.name,
                TransmutationDisplayableRecipe transmutationDisplayableRecipe => transmutationDisplayableRecipe
                    .RecipeData.Recipe.name,
                _ => mTitleText.text
            };
        }

        public void Initialize(WorkBenchUI workBenchUI)
        {
            mCraftButton.onClick.AddListener(workBenchUI.TryCraftItem);
        }
    }
}
